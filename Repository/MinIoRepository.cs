using leapcert_back.Dtos.MinIo;
using leapcert_back.Interfaces;
using leapcert_back.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository;

public class MinIoRepository : IMinIoRepository
{
    private readonly IMinioClient minioClient;

    public MinIoRepository(IMinioClient minioClient)
    {
        this.minioClient = minioClient;
    }

    public async Task<IResponses> GetObject([FromQuery] GetObjectDto bucketInfo)
    {
        if (string.IsNullOrEmpty(bucketInfo.bucketId) || bucketInfo.bucketId.Length < 3) 
            return new ErrorResponse(false, 404, "Bucket ID não encontrado ou Bucket ID inválido.");

        if (string.IsNullOrEmpty(bucketInfo.objectName))
            return new ErrorResponse(false, 400, "Objeto não informado.");

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketInfo.bucketId)
            .WithObject(bucketInfo.objectName)
            .WithExpiry(60 * 60);

        var url = minioClient.PresignedGetObjectAsync(args);
        return new SuccessResponse<Task<string>>(true, 200, "Objeto encontrado com sucesso", url);
    }
}