using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Context;
using leapcert_back.Interfaces;
using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository
{
    public class GeneralRepository : IGeneralRespository
    {
        private readonly ApplicationDbContext _context;
        public GeneralRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IResponses> GetGenders()
        {
            var genders = await _context.tb_genero.ToListAsync();

            if (genders == null)
                return new ErrorResponse(false, 400, "Nenhum gênero encontrado");

            return new SuccessResponse<List<Gender>>(true, 200, "Curso encontrado", genders);
        }
    }
}