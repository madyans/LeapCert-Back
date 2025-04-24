using leapcert_back.Dtos.Users;
using leapcert_back.Helpers;
using leapcert_back.Interfaces;
using leapcert_back.Mappers;
using leapcert_back.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static leapcert_back.Responses.ResponseFactory;
using Microsoft.EntityFrameworkCore;
using wsapi.Context;

namespace leapcert_back.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserMapper _mapperUser;
    private readonly JwtService _jwtService;
    
    public UserRepository(ApplicationDbContext context, UserMapper mapperUser, JwtService jwtService)
    {
        _context = context;
        _mapperUser = mapperUser;
        _jwtService = jwtService;
    }
    
    public async Task<IResponses> GetAllAsync()
    {
        ICollection<User> users = await _context.Usuario.ToListAsync();

        if (users.Count == 0)
            return new ErrorResponse(false, 404 ,"Nenhum usuario encontrado");

        return new SuccessResponse<IEnumerable<User>>(true, 200, "Sucesso ao retornar usuarios", users);
    }

    public async Task<IResponses> PostAsync(CreateUserDto user)
    {
        if (user is null)
            return new ErrorResponse(false, 400, "Usuário não pode ser nulo");

        try
        {
            var userMapped = _mapperUser.MappUserDto(user);
            await _context.Usuario.AddAsync(userMapped);
            await _context.SaveChangesAsync();

            return new SuccessResponse<User>(true, 201, "Usuário criado com sucesso", userMapped);
        }
        catch (Exception ex)
        {
            return new ErrorResponse(false, 500, $"Erro ao criar usuário: {ex.Message}");
        }
    }

    public async Task<IResponses> Authenticate(LoginUserDto user, HttpContext context)
    {
        var existedUser = await _context.Usuario
            .FirstOrDefaultAsync(u => u.usuario == user.usuario);

        if (existedUser is null)
            return new ErrorResponse(false, 404, "Nenhum usuario encontrado");
        
        if (existedUser.senha != HelperService.HashMd5(user.senha))
            return new ErrorResponse(false, 400, "Senha incorreta");
        
        CreateUserSessionDTO userSession = new CreateUserSessionDTO(
            existedUser.codigo.ToString(), 
            existedUser.usuario, 
            existedUser.nome
            );

        ReadUserSessionDTO loggedUser = _jwtService.GenerateJwtToken(userSession);
        
        SetTokensInsideCookie(loggedUser.Token, context);
        
        return new SuccessResponse<ReadUserSessionDTO>(true, 200, "Usuário logado com sucesso!", loggedUser);
    }
    
    public void SetTokensInsideCookie(string token, HttpContext context)
    {
        context.Response.Cookies.Append("accessToken", token, new CookieOptions
        {
            Expires = DateTime.Now.AddHours(5),
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }
}