using CommunityToolkit.HighPerformance;
using leapcert_back.Dtos.MinIo;
using leapcert_back.Interfaces;
using leapcert_back.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Encryption;
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

    public async Task<IResponses> GetObject([FromQuery] GetObjectDto dto)
    {
        if (string.IsNullOrEmpty(dto.bucketId) || dto.bucketId.Length < 3)
            return new ErrorResponse(false, 404, "Bucket ID não encontrado ou Bucket ID inválido.");

        if (string.IsNullOrEmpty(dto.objectName))
            return new ErrorResponse(false, 400, "Objeto não informado.");

        var args = new PresignedGetObjectArgs()
            .WithBucket(dto.bucketId)
            .WithObject(dto.objectName)
            .WithExpiry(60 * 60);

        var url = minioClient.PresignedGetObjectAsync(args);
        return new SuccessResponse<Task<string>>(true, 200, "Objeto encontrado com sucesso", url);
    }

    public async Task<IResponses> GetBucketItems([FromQuery] ListObjectsAsDto dto)
    {
        var listArgs = new ListObjectsArgs()
                .WithBucket(dto.bucketName)
                .WithPrefix(dto.prefix)
                .WithRecursive(dto.recursive)
                .WithVersions(dto.versions);

        List<string> list = new List<string>();

        await foreach (var item in minioClient.ListObjectsEnumAsync(listArgs))
        {
            if (!item.IsDir)
            {
                var key = item.Key;
                if (!string.IsNullOrEmpty(dto.prefix) && key.StartsWith(dto.prefix))
                    key = key.Substring(dto.prefix.Length).TrimStart('/');

                list.Add(key);
            }
        }

        return new SuccessResponse<List<string>>(true, 200, "Todos objetos retornados com sucesso", list);
    }

    public async Task<IResponses> CreateFolder(string path, string folderName)
    {
        var args = new PutObjectArgs()
        .WithBucket(_configuration["MinIO:Bucket"])
        .WithObject(path + folderName + "/")
        .WithFileName("n.txt");

        var name = await minioClient.PutObjectAsync(args).ConfigureAwait(false);
        return new SuccessResponse<Minio.DataModel.Response.PutObjectResponse>(true, 200, "Pasta criada com sucesso", name);
    }

    public async Task<IResponses> PutObject(UploadFileDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
            return new ErrorResponse(false, 400, "Arquivo inválido.");

        var path = $"{dto.path}/{dto.File.FileName}";
        var bucket = _configuration["MinIO:Bucket"];

        using var stream = dto.File.OpenReadStream();

        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(path)
            .WithStreamData(stream)
            .WithObjectSize(dto.File.Length)
            .WithContentType(dto.File.ContentType);

        var result = await minioClient.PutObjectAsync(args);

        return new SuccessResponse<Minio.DataModel.Response.PutObjectResponse>(
            true, 200, "Arquivo enviado com sucesso", result);
    }
}