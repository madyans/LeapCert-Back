using leapcert_back.Dtos.Class;
using leapcert_back.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace leapcert_back.Mappers;

public static class ClassMapper
{
    public static ReadClassDto ToReadClassDto(this UserClass dto)
    {
        return new ReadClassDto()
        {
            codigo = dto.ClassJoin.codigo,
            codigo_professor = dto.codigo_usuario,
            nome = dto.ClassJoin.nome,
            descricao = dto.ClassJoin.descricao,
            avaliacao = dto.ClassJoin.avaliacao ?? "0.0",
            created_at = dto.ClassJoin.created_at,
            codigo_genero = dto.ClassJoin.genero,
            genero = dto.ClassJoin.GenderJoin.nome,
            path = dto.ClassJoin.PathJoin != null ? dto.ClassJoin.PathJoin.path : null
        };
    }

    public static ReadTeacherClassDto ToReadTeacherClassDto(this UserClass dto)
    {
        return new ReadTeacherClassDto()
        {
            codigo_curso = dto.codigo_curso,
            codigo_professor = dto.codigo_usuario,
            nome_curso = dto.ClassJoin.nome,
            nome_professor = dto.UserJoin.nome
        };
    }

    public static ReadAllClassesDto ToReadAllClassesDto(this UserClass dto)
    {
        return new ReadAllClassesDto()
        {
            codigo_curso = dto.codigo_curso,
            nome_curso = dto.ClassJoin.nome,
        };
    }

    public static Class ToCreateClassDto(this CreateClassDto dto)
    {
        return new Class()
        {
            descricao = dto.descricao,
            avaliacao = null,
            created_at = DateTime.UtcNow,
            nome = dto.nome,
            genero = dto.genero
        };
    }
}