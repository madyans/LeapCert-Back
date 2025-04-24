using leapcert_back.Dtos.Users;
using leapcert_back.Helpers;
using leapcert_back.Models;

namespace leapcert_back.Mappers;

public class UserMapper
{
    public User MappUserDto(CreateUserDto user)
    {
        return new User
        {
            nome = user.nome,
            email = user.email,
            usuario = user.usuario,
            senha = HelperService.HashMd5(user.senha),
            avaliacao = 0,
            created_at = DateTime.Now,
            email_boas_vindas_enviado = false,
            perfil = user.perfil,
        };
    }
}