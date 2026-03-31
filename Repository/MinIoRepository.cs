using leapcert_back.Dtos.MinIo;
using leapcert_back.Interfaces;
using leapcert_back.Responses;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository;

public class MinIoRepository : IMinIoRepository
{
    private readonly IMinioClient minioClient;
    private readonly IConfiguration _configuration;

    public MinIoRepository(IMinioClient minioClient, IConfiguration configuration)
    {
        this.minioClient = minioClient;
        _configuration = configuration;
    }

    private static string NormalizeFolderPrefix(string path, string folderName)
    {
        var root = string.IsNullOrWhiteSpace(path) || path == "/"
            ? ""
            : path.Trim().Trim('/').Replace('\\', '/');
        var name = (folderName ?? "").Trim().Trim('/').Replace('\\', '/');
        if (string.IsNullOrEmpty(name))
            return "";
        return string.IsNullOrEmpty(root) ? $"{name}/" : $"{root}/{name}/";
    }

    private static string NormalizeObjectKey(string folderPath, string fileName)
    {
        var folder = (folderPath ?? "").Trim().Trim('/').Replace('\\', '/');
        var safeName = Path.GetFileName(fileName ?? "");
        foreach (var c in new[] { '<', '>', ':', '"', '|', '?', '*', '\0' })
            safeName = safeName.Replace(c, '_');
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = "arquivo.bin";
        return string.IsNullOrEmpty(folder) ? safeName : $"{folder}/{safeName}";
    }

    private async Task EnsureBucketAsync(string bucket)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucket);
        var exists = await minioClient.BucketExistsAsync(existsArgs).ConfigureAwait(false);
        if (!exists)
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket)).ConfigureAwait(false);
        }
    }

    public async Task<IResponses> GetObject([FromQuery] GetObjectDto dto)
    {
        if (string.IsNullOrEmpty(dto.objectName))
            return new ErrorResponse(false, 400, "Objeto não informado.");

        var objectKey = Uri.UnescapeDataString(dto.objectName).TrimStart('/');

        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_configuration["MinIO:Bucket"])
                .WithObject(objectKey)
                .WithExpiry(60 * 60);

            var url = await minioClient.PresignedGetObjectAsync(args).ConfigureAwait(false);
            return new SuccessResponse<string>(true, 200, "Objeto encontrado com sucesso", url);
        }
        catch (Exception ex)
        {
            return new ErrorResponse(false, 502, $"MinIO: {ex.Message}");
        }
    }

    public async Task<IResponses> GetBucketItems([FromQuery] ListObjectsAsDto dto) // para consultar os objetos dentro de uma pasta => folderName/
    {
        var listArgs = new ListObjectsArgs()
                .WithBucket(_configuration["MinIO:Bucket"])
                .WithPrefix(dto.prefix)
                .WithRecursive(dto.recursive)
                .WithVersions(dto.versions);

        List<BucketItemDto> list = new List<BucketItemDto>();

        await foreach (var item in minioClient.ListObjectsEnumAsync(listArgs))
        {
            if (!item.IsDir)
            {
                var key = item.Key;
                if (!string.IsNullOrEmpty(dto.prefix) && key.StartsWith(dto.prefix))
                    key = key.Substring(dto.prefix.Length).TrimStart('/');

                if (!string.IsNullOrEmpty(key))
                {
                    StatObjectArgs statObjectArgs = new StatObjectArgs()
                        .WithBucket(_configuration["MinIO:Bucket"])
                        .WithObject(dto.prefix + key);

                    ObjectStat objectStat = await minioClient.StatObjectAsync(statObjectArgs);
                    var newObject = new BucketItemDto()
                    {
                        ObjectName = key,
                        eTag = objectStat.ETag,
                        contentType = objectStat.ContentType
                    };
                    list.Add(newObject);
                }
            }
        }

        return new SuccessResponse<List<BucketItemDto>>(true, 200, "Todos objetos retornados com sucesso", list);
    }

    public async Task<IResponses> CreateFolder(string path, string folderName)
    {
        var bucket = _configuration["MinIO:Bucket"];
        var folderKey = NormalizeFolderPrefix(path ?? "", folderName ?? "");
        if (string.IsNullOrEmpty(folderKey))
            return new ErrorResponse(false, 400, "Nome da pasta inválido.");

        try
        {
            await EnsureBucketAsync(bucket).ConfigureAwait(false);
            var placeholder = new byte[] { 0x20 };
            using var stream = new MemoryStream(placeholder, writable: false);
            var args = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(folderKey)
                .WithStreamData(stream)
                .WithObjectSize(placeholder.Length)
                .WithContentType("application/octet-stream");

            var name = await minioClient.PutObjectAsync(args).ConfigureAwait(false);
            return new SuccessResponse<Minio.DataModel.Response.PutObjectResponse>(true, 200, "Pasta criada com sucesso", name);
        }
        catch (Exception ex)
        {
            return new ErrorResponse(false, 502, $"MinIO: não foi possível criar a pasta ({ex.Message})");
        }
    }

    public async Task<IResponses> PutObject(UploadFileDto dto)
    {
        if (dto == null || dto.File == null || dto.File.Length == 0)
            return new ErrorResponse(false, 400, "Arquivo inválido.");

        if (string.IsNullOrWhiteSpace(dto.path))
            return new ErrorResponse(false, 400, "Informe o caminho (path) da pasta do curso.");

        var bucket = _configuration["MinIO:Bucket"];
        if (string.IsNullOrWhiteSpace(bucket))
            return new ErrorResponse(false, 500, "Bucket MinIO não configurado.");

        var objectKey = NormalizeObjectKey(dto.path, dto.File.FileName);
        var contentType = string.IsNullOrWhiteSpace(dto.File.ContentType)
            ? "application/octet-stream"
            : dto.File.ContentType;

        try
        {
            await EnsureBucketAsync(bucket).ConfigureAwait(false);

            using var stream = dto.File.OpenReadStream();
            var args = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(dto.File.Length)
                .WithContentType(contentType);

            var result = await minioClient.PutObjectAsync(args).ConfigureAwait(false);
            return new SuccessResponse<Minio.DataModel.Response.PutObjectResponse>(true, 200, "Arquivo enviado com sucesso", result);
        }
        catch (Exception ex)
        {
            return new ErrorResponse(false, 502, $"MinIO: falha ao enviar arquivo ({ex.Message})");
        }
    }
}