using leapcert_back.Dtos.MinIo;
using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers;

[ApiController]
[Route("api/minio")]
public class MinIoController : ControllerBase
{
    private readonly IMinIoRepository _minioRepository;

    public MinIoController(IMinIoRepository minioRepository)
    {
        _minioRepository = minioRepository;
    }

    [Authorize]
    [HttpGet("/objects/getObject")]
    public async Task<IActionResult> GetUrl([FromQuery] GetObjectDto bucketInfo)
    {
        var result = await _minioRepository.GetObject(bucketInfo);

        if (result == null) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("/objects/getAllObjects")]
    public async Task<IActionResult> ListBucketContents([FromQuery] ListObjectsAsDto infos)
    {
        var result = await _minioRepository.GetBucketItems(infos);

        if (result == null) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("createFolder")]
    public async Task<IActionResult> CreateFolder(string path, string folderName)
    {
        var result = await _minioRepository.CreateFolder(path, folderName);

        if (result == null) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }
}