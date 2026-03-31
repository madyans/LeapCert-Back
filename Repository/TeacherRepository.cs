using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Context;
using leapcert_back.Dtos.Class;
using leapcert_back.Dtos.Users;
using leapcert_back.Interfaces;
using leapcert_back.Mappers;
using leapcert_back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private ApplicationDbContext _context;
        private readonly IMinIoRepository _minioRepository;

        public TeacherRepository(ApplicationDbContext context, IMinIoRepository minioRepository)
        {
            _context = context;
            _minioRepository = minioRepository;
        }

        public async Task<IResponses> GetAllClasses(int id)
        {
            ICollection<UserClass> course = await _context.tb_usuario_curso
                .Include(uc => uc.ClassJoin)
                .Include(uc => uc.ClassJoin)
                .Where(uc => uc.codigo_usuario == id)
                .ToListAsync();

            if (course == null)
                return new ErrorResponse(false, 400, "Nenhum curso encontrado");

            var mappedUserClasses = course.Select(uc => uc.ToReadAllClassesDto());

            return new SuccessResponse<IEnumerable<ReadAllClassesDto>>(true, 200, "Cursos encontrados com sucesso", mappedUserClasses);
        }

        public async Task<IResponses> CreateClass([FromBody] CreateClassDto dto)
        {
            if (dto == null)
                return new ErrorResponse(false, 400, "Informações do curso não podem ser nulas");

            if (dto.codigo_professor < 1)
                return new ErrorResponse(false, 400, "Código do professor inválido. Faça login novamente.");

            var professorExists = await _context.Usuario.AnyAsync(u => u.codigo == dto.codigo_professor);
            if (!professorExists)
                return new ErrorResponse(false, 400, "Professor não encontrado no sistema. Faça login novamente.");

            if (dto.genero is not int gen || gen < 1)
                return new ErrorResponse(false, 400, "Selecione uma categoria válida.");

            var genderExists = await _context.tb_genero.AnyAsync(g => g.codigo == gen);
            if (!genderExists)
                return new ErrorResponse(false, 400, "Categoria inválida.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var classMapped = dto.ToCreateClassDto();

                await _context.tb_curso.AddAsync(classMapped);
                await _context.SaveChangesAsync();

                if (classMapped.codigo == 0)
                {
                    await transaction.RollbackAsync();
                    return new ErrorResponse(false, 500, "Erro interno: código do curso não foi gerado.");
                }

                var minioFolder = await _minioRepository.CreateFolder("/", dto.nome);
                if (!minioFolder.Flag)
                {
                    await transaction.RollbackAsync();
                    return minioFolder;
                }

                UserClass newUserClass = new UserClass()
                {
                    codigo_curso = classMapped.codigo,
                    codigo_usuario = dto.codigo_professor,
                    data_matricula = DateTime.UtcNow
                };

                ClassPath newClassPath = new ClassPath()
                {
                    codigo_curso = classMapped.codigo,
                    path = dto.nome + "/",
                };

                await _context.tb_usuario_curso.AddAsync(newUserClass);
                await _context.tb_curso_path.AddAsync(newClassPath);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SuccessResponse<Class>(true, 200, "Curso criado com sucesso", classMapped);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return new ErrorResponse(false, 500, $"Erro ao criar curso: {e.Message}");
            }
        }
    }
}