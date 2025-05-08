using leapcert_back.Dtos.MinIo;
using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers;

[ApiController]
[Route("api/minio")]
public class MinIoController: ControllerBase
{
    private readonly IMinIoRepository _minioRepository;

    public MinIoController(IMinIoRepository minioRepository)
    {
        _minioRepository = minioRepository;
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUrl([FromQuery] GetObjectDto bucketInfo)
    {
        var result = await _minioRepository.GetObject(bucketInfo);

        if (result == null) return  ResponseHelper.HandleError(this, result);
        
        return Ok(result);
    }
}