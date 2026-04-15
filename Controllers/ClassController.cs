using System.Security.Claims;
using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static leapcert_back.Responses.ResponseFactory;

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

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetClasses()
    {
        var result = await _classRepository.GetAllAsync();

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClass(int id)
    {
        var codigo = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("codigo");
        if (string.IsNullOrEmpty(codigo) || !int.TryParse(codigo, out var userId))
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.GetByIdAsync(id, userId);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("getTeacherClass/{id}")]
    public async Task<IActionResult> GetTeacherByClass(int id)
    {
        var result = await _classRepository.GetTeacherByClass(id);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }
}