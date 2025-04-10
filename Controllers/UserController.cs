using System.ComponentModel.Design;
using leapcert_back.Dtos.Users;
using leapcert_back.Helpers;
using leapcert_back.Models;
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
    
    public UserController(ApplicationDbContext context, ILogger<UserController> logger, HelperService helperService )
    {
        _context = context;
        _logger = logger;
        _helperService = helperService;
    }

    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var response = new Response<List<Usuario>>();

        try
        {
            var users = await _context.Usuario.ToListAsync();

            if (users == null || users.Count == 0)
            {
                response.Success = true;
                response.Message = "Nenhum usuário encontrado";
                response.Data = null;
                return StatusCode(204, response);
            }
            response.Success = true;
            response.Message = "Usuário encontrados com sucesso";
            response.Data = users;
            return Ok(users);
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Message = e.Message;
            return StatusCode(500, response);
        }
    }

    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDto usuario)
    {
        var response = new Response<Usuario>();

        try
        {
            var existingUser = await _context.Usuario
                .FirstOrDefaultAsync(u => u.email == usuario.email);

            if (existingUser != null)
            {
                response.Success = false;
                response.Message = "E-mail de usuário já cadastrado, por favor cadastre outro";
                return StatusCode(409, response);
            }
            
            var newUser = new Usuario
            {
                nome = usuario.nome,
                email = usuario.email,
                usuario = usuario.usuario,
                senha = _helperService.HashMd5(usuario.senha),
                avaliacao = 0,
                created_at = DateTime.Now,
                perfil = usuario.perfil,
            };

            _context.Usuario.Add(newUser);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "Usuário cadastrado com sucesso";
            response.Data = newUser;
            return Ok(response);
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Message = e.Message;
            return StatusCode(500, response);
        }
    }
}