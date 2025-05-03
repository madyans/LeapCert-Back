using leapcert_back.Helper;
using leapcert_back.Interfaces;
using leapcert_back.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers;

[Route("api/module")]
[ApiController]
public class ModuleController : ControllerBase
{
    private readonly IModulesRepository _moduleRepository;

    public ModuleController(IModulesRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllModules()
    {
        var result = await _moduleRepository.GetAllAsync();

        if (result.Flag == false) ResponseHelper.HandleError(this, result);

        return Ok(result);
    }
}