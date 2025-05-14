using leapcert_back.Dtos.MinIo;
using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using leapcert_back.Responses;

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
    [HttpGet("objects/getObject")]
    public async Task<IActionResult> GetUrl([FromQuery] GetObjectDto bucketInfo)
    {
        var result = await _minioRepository.GetObject(bucketInfo);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("objects/getAllObjects")]
    public async Task<IActionResult> ListBucketContents([FromQuery] ListObjectsAsDto infos)
    {
        var result = await _minioRepository.GetBucketItems(infos);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("createFolder")]
    public async Task<IActionResult> CreateFolder(string path, string folderName)
    {
        var result = await _minioRepository.CreateFolder(path, folderName);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [Consumes("multipart/form-data")]
    [HttpPost("sendObject")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileDto dto)
    {
        var result = await _minioRepository.PutObject(dto);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("proxyImage")]
    public async Task<IActionResult> ProxyImage([FromQuery] GetObjectDto bucketInfo)
    {
        var result = await _minioRepository.GetObject(bucketInfo);

        if (result is not ResponseFactory.SuccessResponse<Task<string>> typedResult)
            return ResponseHelper.HandleError(this, result);

        var url = await typedResult.Data;

        using var httpClient = new HttpClient();
        var imageBytes = await httpClient.GetByteArrayAsync(url);

        var contentType = GetContentType(bucketInfo.objectName);
        return File(imageBytes, contentType);
    }

    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}