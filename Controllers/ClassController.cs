using System.Security.Claims;
using leapcert_back.Dtos.Class;
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
        var result = await _classRepository.GetAllAsync(GetAuthenticatedUserId());

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("student/courses")]
    public async Task<IActionResult> GetStudentCourses()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.GetStudentCoursesAsync(userId.Value);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClass(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.GetByIdAsync(id, userId.Value);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/connect")]
    public async Task<IActionResult> ConnectToCourse(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.ConnectToCourseAsync(id, userId.Value);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/learning-path/{itemId}/complete")]
    public async Task<IActionResult> CompleteLearningPathItem(int id, int itemId)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.CompleteLearningPathItemAsync(id, itemId, userId.Value);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}/learning-path/{itemId}/complete")]
    public async Task<IActionResult> UncompleteLearningPathItem(int id, int itemId)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.UncompleteLearningPathItemAsync(id, itemId, userId.Value);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/rating")]
    public async Task<IActionResult> UpsertCourseRating(int id, [FromBody] UpsertCourseRatingDto dto)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.UpsertCourseRatingAsync(id, userId.Value, dto);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}/topics")]
    public async Task<IActionResult> UpdateCourseTopics(int id, [FromBody] CourseTopicsDto dto)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.UpdateCourseTopicsAsync(id, userId.Value, dto);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/notes")]
    public async Task<IActionResult> CreateCourseNote(int id, [FromBody] UpsertCourseUserNoteDto dto)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.CreateCourseNoteAsync(id, userId.Value, dto);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/forum-topics")]
    public async Task<IActionResult> CreateCourseForumTopic(int id, [FromBody] CreateCourseForumTopicDto dto)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.CreateCourseForumTopicAsync(id, userId.Value, dto);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}/notes/{noteId}")]
    public async Task<IActionResult> UpdateCourseNote(int id, int noteId, [FromBody] UpsertCourseUserNoteDto dto)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.UpdateCourseNoteAsync(id, noteId, userId.Value, dto);

        if (!result.Flag) return ResponseHelper.HandleError(this, result);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}/notes/{noteId}")]
    public async Task<IActionResult> DeleteCourseNote(int id, int noteId)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new ErrorResponse(false, 401, "Sessão inválida. Faça login novamente."));

        var result = await _classRepository.DeleteCourseNoteAsync(id, noteId, userId.Value);

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

    private int? GetAuthenticatedUserId()
    {
        var codigo = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("codigo");

        return string.IsNullOrEmpty(codigo) || !int.TryParse(codigo, out var userId)
            ? null
            : userId;
    }
}
