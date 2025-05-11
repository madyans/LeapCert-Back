using System.Collections;
using leapcert_back.Interfaces;
using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Context;
using leapcert_back.Dtos.Class;
using leapcert_back.Mappers;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository;

public class ClassRepository : IClassRepository
{
    private readonly ApplicationDbContext _context;

    public ClassRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IResponses> GetAllAsync()
    {
        ICollection<Class> classes = await _context.tb_curso
            .Include(course => course.GenderJoin)
            .ToListAsync();

        if (classes.Count == 0)
            return new ErrorResponse(false, 400, "Nenhum curso encontrado");

        var mappedClasses = classes.Select(c => c.ToReadClassDto());

        return new SuccessResponse<IEnumerable>(true, 200, "Cursos encontrados", mappedClasses);
    }

    public async Task<IResponses> GetByIdAsync(int id)
    {
        Class? course = await _context.tb_curso
            .Include(course => course.GenderJoin)
            .FirstOrDefaultAsync(course => course.codigo == id);

        if (course == null)
            return new ErrorResponse(false, 400, "Nenhum curso encontrado nesse id");

        var mappedClass = course.ToReadClassDto();

        return new SuccessResponse<ReadClassDto>(true, 200, "Curso encontrado", mappedClass);
    }

    public async Task<IResponses> GetTeacherByClass(int id)
    {
        UserClass? userClass = await _context.tb_usuario_curso
            .Include(uc => uc.ClassJoin)
            .Include(uc => uc.UserJoin)
            .FirstOrDefaultAsync(course => course.codigo_usuario == id);

        if (userClass == null)
            return new ErrorResponse(false, 400, "Nenhum professor encontrado");

        var mappadUserClass = userClass.ToReadTeacherClassDto();

        return new SuccessResponse<ReadTeacherClassDto>(true, 200, "Professor encontrado", mappadUserClass);
    }
}