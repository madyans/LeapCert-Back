using System.ComponentModel.Design;
using leapcert_back.Dtos.Users;
using leapcert_back.Helper;
using leapcert_back.Helpers;
using leapcert_back.Interfaces;
using leapcert_back.Models;
using static leapcert_back.Responses.ResponseFactory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wsapi.Context;

namespace leapcert_back.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;
    private readonly HelperService _helperService;
    private readonly IUserRepository _userRepository;
    
    public UserController(
        ApplicationDbContext context, 
        ILogger<UserController> logger, 
        HelperService helperService, 
        IUserRepository userRepository 
        )
    {
        _context = context;
        _logger = logger;
        _helperService = helperService;
        _userRepository = userRepository;
    }

    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userRepository.GetAllAsync();

        if (result.Flag == false) ResponseHelper.HandleError(this, result);
                
        return Ok(result);
    }

    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDto usuario)
    {
        var result = await _userRepository.PostAsync(usuario);
        
        if (result.Flag == false) ResponseHelper.HandleError(this, result);
        
        return Ok(result);
    }
}