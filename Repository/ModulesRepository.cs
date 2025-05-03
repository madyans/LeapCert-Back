using leapcert_back.Interfaces;
using leapcert_back.Models;
using leapcert_back.Responses;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Context;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository;

public class ModulesRepository : IModulesRepository
{
    private readonly ApplicationDbContext _context;

    public ModulesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IResponses> GetAllAsync()
    {
        ICollection<Modules> modules = await _context.tb_modulos
            .OrderBy(module => module.ordem)
            .ToListAsync();

        if (modules.Count == 0)
            return new ErrorResponse(false, 400, "Nenhum módulo encontrado");

        return new SuccessResponse<ICollection<Modules>>(true, 200, "Móduloes encontrados com sucesso", modules);
    }
}