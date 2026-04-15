using leapcert_back.Context;
using leapcert_back.Dtos.Class;
using leapcert_back.Dtos.MinIo;
using leapcert_back.Interfaces;
using leapcert_back.Mappers;
using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository;

public class ClassRepository : IClassRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMinIoRepository _minioRepository;

    public ClassRepository(ApplicationDbContext context, IMinIoRepository minioRepository)
    {
        _context = context;
        _minioRepository = minioRepository;
    }

    public async Task<IResponses> GetAllAsync()
    {
        var courses = await _context.tb_curso
            .AsNoTracking()
            .Include(c => c.GenderJoin)
            .Include(c => c.PathJoin)
            .OrderByDescending(c => c.created_at)
            .ToListAsync();

        var enrollments = await _context.tb_usuario_curso.AsNoTracking().ToListAsync();
        var profLookup = enrollments
            .GroupBy(uc => uc.codigo_curso)
            .ToDictionary(g => g.Key, g => g.First().codigo_usuario);

        var catalog = new List<ReadClassCatalogDto>();

        foreach (var course in courses)
        {
            profLookup.TryGetValue(course.codigo, out var profId);
            var dto = course.ToCatalogDto(profId);

            var rawPath = dto.path?.Trim();
            if (!string.IsNullOrEmpty(rawPath))
            {
                var normalizedPrefix = rawPath.EndsWith('/') ? rawPath : rawPath + "/";
                var listResult = await _minioRepository.GetBucketItems(new ListObjectsAsDto
                {
                    prefix = normalizedPrefix,
                    recursive = true,
                    versions = false,
                });

                if (listResult is SuccessResponse<List<BucketItemDto>> ok && ok.Data != null)
                {
                    dto.objects = ok.Data
                        .Where(o =>
                            !string.IsNullOrEmpty(o.ObjectName) &&
                            !o.ObjectName.EndsWith("/", StringComparison.Ordinal))
                        .ToList();
                }
            }

            catalog.Add(dto);
        }

        return new SuccessResponse<List<ReadClassCatalogDto>>(true, 200, "Cursos encontrados", catalog);
    }

    private async Task<bool> UserHasPublishedActiveCourseAsync(int userId)
    {
        return await _context.tb_usuario_curso
            .AsNoTracking()
            .AnyAsync(uc => uc.codigo_usuario == userId);
    }

    public async Task<IResponses> GetByIdAsync(int id, int requestingUserId)
    {
        var course = await _context.tb_usuario_curso
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.GenderJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.PathJoin)
            .FirstOrDefaultAsync(uc => uc.codigo_curso == id);

        if (course == null)
            return new ErrorResponse(false, 400, "Nenhum curso encontrado nesse id");

        var isCourseOwner = course.codigo_usuario == requestingUserId;
        if (!isCourseOwner && !await UserHasPublishedActiveCourseAsync(requestingUserId))
        {
            return new ErrorResponse(
                false,
                403,
                "Apenas professores com ao menos um curso cadastrado podem ver detalhes de cursos de outros.");
        }

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
