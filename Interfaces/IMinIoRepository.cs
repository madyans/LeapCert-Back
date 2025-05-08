using leapcert_back.Dtos.MinIo;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Interfaces;

public interface IMinIoRepository
{
    Task<IResponses> GetObject([FromQuery] GetObjectDto bucketInfo);
}