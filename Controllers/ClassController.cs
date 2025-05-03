using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers;

[Route("api/class")]
[ApiController]
public class ClassController: ControllerBase
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

        if (result.Flag == false) ResponseHelper.HandleError(this, result);
        
        return Ok(result);
    }
}