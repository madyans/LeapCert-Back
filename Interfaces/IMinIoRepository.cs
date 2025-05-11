using leapcert_back.Dtos.MinIo;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Interfaces;

public interface IMinIoRepository
{
    Task<IResponses> GetObject([FromQuery] GetObjectDto dto);
    Task<IResponses> GetBucketItems([FromQuery] ListObjectsAsDto dto);
    Task<IResponses> CreateFolder(string path, string folderName);
    Task<IResponses> PutObject(UploadFileDto dto);
}