using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers;

[Route("api/class")]
[ApiController]
public class ClassController : ControllerBase
{
    private readonly IClassRepository _classRepository;

    public ClassController(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetClasses()
    {
        var result = await _classRepository.GetAllAsync();

        if (!result.Flag) ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClass(int id)
    {
        var result = await _classRepository.GetByIdAsync(id);

        if (!result.Flag) ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("getTeacherClass/{id}")]
    public async Task<IActionResult> GetTeacherByClass(int id)
    {
        var result = await _classRepository.GetTeacherByClass(id);

        if (!result.Flag) ResponseHelper.HandleError(this, result);

        return Ok(result);
    }
}