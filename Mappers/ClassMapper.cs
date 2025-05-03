using leapcert_back.Dtos.Class;
using leapcert_back.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace leapcert_back.Mappers;

public static class ClassMapper
{
    public static ReadClassDto ToReadClassDto(this Class dto)
    {
        return new ReadClassDto()
        {   
            codigo = dto.codigo,
            nome = dto.nome,
            descricao = dto.descricao,
            avaliacao = dto.avaliacao ?? "0.0",
            created_at = dto.created_at,
            codigo_genero = dto.genero,
            genero = dto.GenderJoin.nome,
        };
    }
}