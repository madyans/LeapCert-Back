using System.ComponentModel.Design;
using leapcert_back.Dtos.Users;
using leapcert_back.Helper;
using leapcert_back.Helpers;
using leapcert_back.Interfaces;
using leapcert_back.Models;
using Microsoft.AspNetCore.Authorization;
using static leapcert_back.Responses.ResponseFactory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Context;

namespace leapcert_back.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    
    public UserController(
        IUserRepository userRepository
        )
    {
        _userRepository = userRepository;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> AuthUser([FromBody] LoginUserDto user)
    {
        var result = await _userRepository.Authenticate(user, HttpContext);
        
        if (result.Flag == false) ResponseHelper.HandleError(this, result);
                
        return Ok(result);
    }

    [HttpGet("validateToken")]
    public IActionResult ValidateToken([FromQuery] string token)
    {
        var result = _userRepository.ValidateToken(token);
        
        if(result.Flag == false) ResponseHelper.HandleError(this, result);
        
        return Ok(result);
    }
    
    [Authorize]
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