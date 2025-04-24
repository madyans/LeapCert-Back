using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using leapcert_back.Dtos.Users;
using Microsoft.IdentityModel.Tokens;

namespace leapcert_back.Helpers;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ReadUserSessionDTO GenerateJwtToken(CreateUserSessionDTO user)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("codigo", user.codigo),
            new Claim("usuario", user.usuario),
            new Claim("nome", user.nome),
            new Claim("loginTimeStamp", DateTime.UtcNow.ToString())
        });

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
        var signInCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(5),
            claims: identity.Claims,
            signingCredentials: signInCredentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new ReadUserSessionDTO(
            user.codigo,
            user.usuario,
            user.nome,
            DateTime.UtcNow.ToString(),
            tokenString
        );
    }
}