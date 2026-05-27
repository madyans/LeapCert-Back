using leapcert_back.Context;
using leapcert_back.Dtos.Class;
using leapcert_back.Dtos.MinIo;
using leapcert_back.Interfaces;
using leapcert_back.Mappers;
using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository;

public class ClassRepository : IClassRepository
{
    private const string CourseAccessForbiddenMessage = "Conecte-se ao curso para acessar este recurso.";
    private const string ConnectedStatus = "connected";

    private readonly ApplicationDbContext _context;
    private readonly IMinIoRepository _minioRepository;

    public ClassRepository(ApplicationDbContext context, IMinIoRepository minioRepository)
    {
        _context = context;
        _minioRepository = minioRepository;
    }

    public async Task<IResponses> GetAllAsync(int? requestingUserId = null)
    {
        var courses = await _context.tb_curso
            .AsNoTracking()
            .Include(c => c.GenderJoin)
            .Include(c => c.PathJoin)
            .OrderByDescending(c => c.created_at)
            .ToListAsync();

        var enrollments = await _context.tb_usuario_curso
            .AsNoTracking()
            .Include(uc => uc.UserJoin)
            .ToListAsync();
        var ownerLookup = enrollments
            .GroupBy(uc => uc.codigo_curso)
            .ToDictionary(g => g.Key, g => g.First());
        var connectedCourseIds = requestingUserId == null
            ? new HashSet<int>()
            : await GetConnectedCourseIdsAsync(requestingUserId.Value);
        var progressLookup = requestingUserId == null
            ? new Dictionary<int, int>()
            : await GetProgressLookupAsync(requestingUserId.Value);

        var catalog = new List<ReadClassCatalogDto>();

        foreach (var course in courses)
        {
            ownerLookup.TryGetValue(course.codigo, out var owner);
            var ownerId = owner?.codigo_usuario ?? 0;
            var isOwner = requestingUserId != null && ownerId == requestingUserId.Value;
            var isConnected = connectedCourseIds.Contains(course.codigo);
            progressLookup.TryGetValue(course.codigo, out var progressPercent);
            var dto = course.ToCatalogDto(ownerId, owner?.UserJoin?.nome ?? "", isOwner, isConnected, progressPercent);
            if (!dto.can_access_content)
            {
                dto.path = null;
            }

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

    public async Task<IResponses> GetByIdAsync(int id, int requestingUserId)
    {
        var courseOwnerRow = await _context.tb_usuario_curso
            .AsNoTracking()
            .Include(uc => uc.UserJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.GenderJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.PathJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.SectionsJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.LearningPathJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.ForumTopicsJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.AssessmentItemsJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.CertificatesJoin)
            .Include(uc => uc.ClassJoin)
            .ThenInclude(c => c.TeacherContactJoin)
            .Where(uc => uc.codigo_curso == id)
            .OrderBy(uc => uc.codigo_usuario)
            .FirstOrDefaultAsync();

        if (courseOwnerRow == null)
            return new ErrorResponse(false, 400, "Nenhum curso encontrado nesse id");

        var isCourseOwner = courseOwnerRow.codigo_usuario == requestingUserId;
        var isConnected = await IsUserConnectedToCourseAsync(id, requestingUserId);
        var canAccessContent = isCourseOwner || isConnected;

        var currentUserRating = await _context.tb_curso_avaliacao
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.codigo_curso == id && cr.codigo_usuario == requestingUserId);

        var completedItems = await GetCompletedLearningPathItemIdsAsync(id, requestingUserId);
        var progress = CalculateProgress(courseOwnerRow.ClassJoin.LearningPathJoin.Count, completedItems.Count);
        var mappedClass = courseOwnerRow.ToReadClassDto(
            currentUserRating?.nota,
            currentUserRating?.comentario,
            completedItems,
            isCourseOwner,
            isConnected,
            canAccessContent,
            progress);
        mappedClass.is_owner = isCourseOwner;
        mappedClass.is_connected = isConnected;
        mappedClass.can_access_content = canAccessContent;
        mappedClass.connection_status = isCourseOwner ? "owner" : isConnected ? ConnectedStatus : "available";
        mappedClass.progresso_usuario = progress;
        mappedClass.path = canAccessContent ? mappedClass.path : null;

        if (canAccessContent)
        {
            mappedClass.anotacoes = await _context.tb_curso_anotacao_usuario
                .AsNoTracking()
                .Where(note => note.codigo_curso == id && note.codigo_usuario == requestingUserId)
                .OrderByDescending(note => note.updated_at)
                .Select(note => new CourseUserNoteDto
                {
                    codigo = note.codigo,
                    codigo_curso = note.codigo_curso,
                    codigo_usuario = note.codigo_usuario,
                    titulo = note.titulo,
                    conteudo = note.conteudo,
                    created_at = note.created_at,
                    updated_at = note.updated_at,
                })
                .ToListAsync();
        }

        return new SuccessResponse<ReadClassDto>(true, 200, "Curso encontrado", mappedClass);
    }

    public async Task<IResponses> GetStudentCoursesAsync(int requestingUserId)
    {
        var ownedRows = await _context.tb_usuario_curso
            .AsNoTracking()
            .Include(uc => uc.UserJoin)
            .Include(uc => uc.ClassJoin).ThenInclude(c => c.GenderJoin)
            .Include(uc => uc.ClassJoin).ThenInclude(c => c.PathJoin)
            .Where(uc => uc.codigo_usuario == requestingUserId)
            .ToListAsync();

        var connectedRows = await _context.tb_curso_conexao_usuario
            .AsNoTracking()
            .Include(connection => connection.ClassJoin).ThenInclude(c => c.GenderJoin)
            .Include(connection => connection.ClassJoin).ThenInclude(c => c.PathJoin)
            .Include(connection => connection.CreatorJoin)
            .Where(connection => connection.codigo_usuario == requestingUserId && connection.status == ConnectedStatus)
            .ToListAsync();

        var progressLookup = await GetProgressLookupAsync(requestingUserId);

        var ownedCourses = ownedRows
            .Select(row =>
            {
                progressLookup.TryGetValue(row.codigo_curso, out var progress);
                return row.ClassJoin.ToCatalogDto(row.codigo_usuario, row.UserJoin?.nome ?? "", isOwner: true, progressPercent: progress);
            })
            .OrderByDescending(course => course.created_at)
            .ToList();

        var connectedCourses = connectedRows
            .Select(row =>
            {
                progressLookup.TryGetValue(row.codigo_curso, out var progress);
                return row.ClassJoin.ToCatalogDto(row.codigo_criador_curso, row.CreatorJoin?.nome ?? "", isConnected: true, progressPercent: progress);
            })
            .OrderByDescending(course => course.created_at)
            .ToList();

        var inProgressCourses = connectedCourses
            .Concat(ownedCourses)
            .Where(course => course.progresso_usuario > 0 && course.progresso_usuario < 100)
            .OrderByDescending(course => course.progresso_usuario)
            .ToList();

        return new SuccessResponse<StudentCoursesDto>(
            true,
            200,
            "Cursos do aluno encontrados",
            new StudentCoursesDto
            {
                cursos_criados = ownedCourses,
                cursos_conectados = connectedCourses,
                cursos_em_andamento = inProgressCourses,
            });
    }

    public async Task<IResponses> ConnectToCourseAsync(int courseId, int requestingUserId)
    {
        var owner = await _context.tb_usuario_curso
            .AsNoTracking()
            .FirstOrDefaultAsync(uc => uc.codigo_curso == courseId);

        if (owner == null)
        {
            return new ErrorResponse(false, 404, "Curso não encontrado.");
        }

        if (owner.codigo_usuario == requestingUserId)
        {
            return new SuccessResponse<CourseConnectionDto>(
                true,
                200,
                "Você é o criador deste curso.",
                new CourseConnectionDto
                {
                    codigo_curso = courseId,
                    connection_status = "owner",
                    is_owner = true,
                    can_access_content = true,
                    progresso = await BuildProgressDtoAsync(courseId, requestingUserId),
                });
        }

        var existingConnection = await _context.tb_curso_conexao_usuario
            .FirstOrDefaultAsync(connection => connection.codigo_curso == courseId && connection.codigo_usuario == requestingUserId);

        if (existingConnection == null)
        {
            var now = DateTime.UtcNow;
            existingConnection = new CourseConnection
            {
                codigo_curso = courseId,
                codigo_usuario = requestingUserId,
                codigo_criador_curso = owner.codigo_usuario,
                status = ConnectedStatus,
                created_at = now,
                updated_at = now,
            };

            await _context.tb_curso_conexao_usuario.AddAsync(existingConnection);
        }
        else
        {
            existingConnection.status = ConnectedStatus;
            existingConnection.codigo_criador_curso = owner.codigo_usuario;
            existingConnection.updated_at = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new SuccessResponse<CourseConnectionDto>(
            true,
            200,
            "Conexão com o curso criada com sucesso.",
            new CourseConnectionDto
            {
                codigo_curso = courseId,
                connection_status = ConnectedStatus,
                is_connected = true,
                can_access_content = true,
                progresso = await BuildProgressDtoAsync(courseId, requestingUserId),
            });
    }

    public async Task<IResponses> CompleteLearningPathItemAsync(int courseId, int itemId, int requestingUserId)
    {
        if (!await CanAccessCourseContentAsync(courseId, requestingUserId))
        {
            return new ErrorResponse(false, 403, CourseAccessForbiddenMessage);
        }

        var itemExists = await _context.tb_curso_trilha
            .AnyAsync(item => item.codigo == itemId && item.codigo_curso == courseId);

        if (!itemExists)
        {
            return new ErrorResponse(false, 404, "Item da trilha não encontrado.");
        }

        var now = DateTime.UtcNow;
        var progress = await _context.tb_curso_trilha_progresso_usuario
            .FirstOrDefaultAsync(item => item.codigo_usuario == requestingUserId && item.codigo_trilha_item == itemId);

        if (progress == null)
        {
            progress = new CourseLearningPathProgress
            {
                codigo_usuario = requestingUserId,
                codigo_curso = courseId,
                codigo_trilha_item = itemId,
                concluido = true,
                concluido_em = now,
                created_at = now,
                updated_at = now,
            };
            await _context.tb_curso_trilha_progresso_usuario.AddAsync(progress);
        }
        else
        {
            progress.concluido = true;
            progress.concluido_em ??= now;
            progress.updated_at = now;
        }

        await _context.SaveChangesAsync();

        return new SuccessResponse<CourseProgressDto>(true, 200, "Item concluído.", await BuildProgressDtoAsync(courseId, requestingUserId));
    }

    public async Task<IResponses> UncompleteLearningPathItemAsync(int courseId, int itemId, int requestingUserId)
    {
        if (!await CanAccessCourseContentAsync(courseId, requestingUserId))
        {
            return new ErrorResponse(false, 403, CourseAccessForbiddenMessage);
        }

        var progress = await _context.tb_curso_trilha_progresso_usuario
            .FirstOrDefaultAsync(item => item.codigo_usuario == requestingUserId && item.codigo_curso == courseId && item.codigo_trilha_item == itemId);

        if (progress != null)
        {
            progress.concluido = false;
            progress.concluido_em = null;
            progress.updated_at = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new SuccessResponse<CourseProgressDto>(true, 200, "Conclusão removida.", await BuildProgressDtoAsync(courseId, requestingUserId));
    }

    public async Task<IResponses> UpdateCourseTopicsAsync(int courseId, int requestingUserId, CourseTopicsDto dto)
    {
        var isCourseOwner = await _context.tb_usuario_curso
            .AsNoTracking()
            .AnyAsync(uc => uc.codigo_curso == courseId && uc.codigo_usuario == requestingUserId);

        if (!isCourseOwner)
        {
            return new ErrorResponse(false, 403, "Apenas o professor do curso pode editar estes tópicos.");
        }

        var courseExists = await _context.tb_curso.AnyAsync(c => c.codigo == courseId);
        if (!courseExists)
        {
            return new ErrorResponse(false, 404, "Curso não encontrado.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.tb_curso_secao.RemoveRange(_context.tb_curso_secao.Where(item => item.codigo_curso == courseId));
            _context.tb_curso_trilha.RemoveRange(_context.tb_curso_trilha.Where(item => item.codigo_curso == courseId));
            _context.tb_curso_forum_topico.RemoveRange(_context.tb_curso_forum_topico.Where(item => item.codigo_curso == courseId));
            _context.tb_curso_avaliacao_item.RemoveRange(_context.tb_curso_avaliacao_item.Where(item => item.codigo_curso == courseId));
            _context.tb_curso_certificado.RemoveRange(_context.tb_curso_certificado.Where(item => item.codigo_curso == courseId));
            _context.tb_curso_professor_contato.RemoveRange(_context.tb_curso_professor_contato.Where(item => item.codigo_curso == courseId));
            await _context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            await _context.tb_curso_secao.AddRangeAsync(NormalizeSections(dto.secoes).Select((section, index) => new CourseSection
            {
                codigo_curso = courseId,
                titulo = section.titulo,
                conteudo = section.conteudo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_trilha.AddRangeAsync(NormalizeLearningPath(dto.trilha).Select((item, index) => new CourseLearningPathItem
            {
                codigo_curso = courseId,
                titulo = item.titulo,
                tipo = item.tipo,
                concluido_padrao = item.concluido_padrao,
                arquivo_nome = item.arquivo_nome,
                arquivo_path = item.arquivo_path,
                arquivo_tipo = item.arquivo_tipo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_forum_topico.AddRangeAsync(NormalizeForumTopics(dto.forum_topicos).Select((topic, index) => new CourseForumTopic
            {
                codigo_curso = courseId,
                autor = topic.autor,
                titulo = topic.titulo,
                resumo = topic.resumo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_avaliacao_item.AddRangeAsync(NormalizeAssessmentItems(dto.avaliacoes_itens).Select((item, index) => new CourseAssessmentItem
            {
                codigo_curso = courseId,
                titulo = item.titulo,
                tipo = item.tipo,
                quantidade_questoes = item.quantidade_questoes,
                duracao = item.duracao,
                prazo = item.prazo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_certificado.AddRangeAsync(NormalizeCertificates(dto.certificados).Select((certificate, index) => new CourseCertificate
            {
                codigo_curso = courseId,
                titulo = certificate.titulo,
                descricao = certificate.descricao,
                status = certificate.status,
                progresso_padrao = certificate.progresso_padrao,
                disponivel_padrao = certificate.disponivel_padrao,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            var contact = NormalizeTeacherContact(dto.contato_professor);
            if (contact != null)
            {
                await _context.tb_curso_professor_contato.AddAsync(new CourseTeacherContact
                {
                    codigo_curso = courseId,
                    nome_professor = contact.nome_professor,
                    subtitulo = contact.subtitulo,
                    mensagem_orientacao = contact.mensagem_orientacao,
                    created_at = now,
                    updated_at = now,
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new SuccessResponse<object>(true, 200, "Tópicos do curso atualizados com sucesso.", new { codigo_curso = courseId });
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return new ErrorResponse(false, 500, $"Erro ao atualizar tópicos do curso: {e.Message}");
        }
    }

    public async Task<IResponses> CreateCourseNoteAsync(int courseId, int requestingUserId, UpsertCourseUserNoteDto dto)
    {
        var title = dto.titulo?.Trim() ?? string.Empty;
        var content = dto.conteudo?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            return new ErrorResponse(false, 400, "Informe título e conteúdo da anotação.");
        }

        var courseExists = await _context.tb_curso.AnyAsync(c => c.codigo == courseId);
        if (!courseExists)
        {
            return new ErrorResponse(false, 404, "Curso não encontrado.");
        }

        if (!await CanAccessCourseContentAsync(courseId, requestingUserId))
        {
            return new ErrorResponse(false, 403, CourseAccessForbiddenMessage);
        }

        var now = DateTime.UtcNow;
        var note = new CourseUserNote
        {
            codigo_curso = courseId,
            codigo_usuario = requestingUserId,
            titulo = title,
            conteudo = content,
            created_at = now,
            updated_at = now,
        };

        await _context.tb_curso_anotacao_usuario.AddAsync(note);
        await _context.SaveChangesAsync();

        return new SuccessResponse<CourseUserNoteDto>(true, 200, "Anotação criada com sucesso.", ToNoteDto(note));
    }

    public async Task<IResponses> CreateCourseForumTopicAsync(int courseId, int requestingUserId, CreateCourseForumTopicDto dto)
    {
        var title = dto.titulo?.Trim() ?? string.Empty;
        var summary = dto.resumo?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(summary))
        {
            return new ErrorResponse(false, 400, "Informe título e descrição da discussão.");
        }

        var courseExists = await _context.tb_curso.AnyAsync(c => c.codigo == courseId);
        if (!courseExists)
        {
            return new ErrorResponse(false, 404, "Curso não encontrado.");
        }

        if (!await CanAccessCourseContentAsync(courseId, requestingUserId))
        {
            return new ErrorResponse(false, 403, CourseAccessForbiddenMessage);
        }

        var author = await _context.Usuario
            .AsNoTracking()
            .Where(user => user.codigo == requestingUserId)
            .Select(user => user.nome)
            .FirstOrDefaultAsync();

        var nextOrder = await _context.tb_curso_forum_topico
            .Where(topic => topic.codigo_curso == courseId)
            .Select(topic => (int?)topic.ordem)
            .MaxAsync() ?? 0;

        var now = DateTime.UtcNow;
        var forumTopic = new CourseForumTopic
        {
            codigo_curso = courseId,
            autor = string.IsNullOrWhiteSpace(author) ? "Usuário" : author,
            titulo = title,
            resumo = summary,
            ordem = nextOrder + 1,
            created_at = now,
            updated_at = now,
        };

        await _context.tb_curso_forum_topico.AddAsync(forumTopic);
        await _context.SaveChangesAsync();

        return new SuccessResponse<CourseForumTopicDto>(true, 200, "Discussão criada com sucesso.", new CourseForumTopicDto
        {
            codigo = forumTopic.codigo,
            autor = forumTopic.autor,
            titulo = forumTopic.titulo,
            resumo = forumTopic.resumo,
            ordem = forumTopic.ordem,
        });
    }

    public async Task<IResponses> UpdateCourseNoteAsync(int courseId, int noteId, int requestingUserId, UpsertCourseUserNoteDto dto)
    {
        var note = await _context.tb_curso_anotacao_usuario
            .FirstOrDefaultAsync(n => n.codigo == noteId && n.codigo_curso == courseId && n.codigo_usuario == requestingUserId);

        if (note == null)
        {
            return new ErrorResponse(false, 404, "Anotação não encontrada.");
        }

        var title = dto.titulo?.Trim() ?? string.Empty;
        var content = dto.conteudo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            return new ErrorResponse(false, 400, "Informe título e conteúdo da anotação.");
        }

        note.titulo = title;
        note.conteudo = content;
        note.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new SuccessResponse<CourseUserNoteDto>(true, 200, "Anotação atualizada com sucesso.", ToNoteDto(note));
    }

    public async Task<IResponses> DeleteCourseNoteAsync(int courseId, int noteId, int requestingUserId)
    {
        var note = await _context.tb_curso_anotacao_usuario
            .FirstOrDefaultAsync(n => n.codigo == noteId && n.codigo_curso == courseId && n.codigo_usuario == requestingUserId);

        if (note == null)
        {
            return new ErrorResponse(false, 404, "Anotação não encontrada.");
        }

        _context.tb_curso_anotacao_usuario.Remove(note);
        await _context.SaveChangesAsync();

        return new SuccessResponse<object>(true, 200, "Anotação removida com sucesso.", new { codigo = noteId });
    }

    public async Task<IResponses> UpsertCourseRatingAsync(int courseId, int requestingUserId, UpsertCourseRatingDto dto)
    {
        if (dto.nota < 1 || dto.nota > 5)
        {
            return new ErrorResponse(false, 400, "A nota deve estar entre 1 e 5.");
        }

        var course = await _context.tb_curso.FirstOrDefaultAsync(c => c.codigo == courseId);
        if (course == null)
        {
            return new ErrorResponse(false, 404, "Curso não encontrado.");
        }

        if (!await CanAccessCourseContentAsync(courseId, requestingUserId))
        {
            return new ErrorResponse(false, 403, CourseAccessForbiddenMessage);
        }

        var now = DateTime.UtcNow;
        var normalizedComment = NormalizeComment(dto.comentario);

        if (_context.Database.IsRelational())
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var response = await PersistRatingAndRefreshAggregateAsync(course, courseId, requestingUserId, dto.nota, normalizedComment, now);
                await transaction.CommitAsync();
                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return await PersistRatingAndRefreshAggregateAsync(course, courseId, requestingUserId, dto.nota, normalizedComment, now);
    }

    private async Task<SuccessResponse<ReadCourseRatingDto>> PersistRatingAndRefreshAggregateAsync(Class course, int courseId, int requestingUserId, decimal nota, string? comentario, DateTime now)
    {
        var persistedRating = await PersistCourseRatingAsync(courseId, requestingUserId, nota, comentario, now);

        var aggregate = await _context.tb_curso_avaliacao
            .Where(cr => cr.codigo_curso == courseId)
            .AverageAsync(cr => cr.nota);

        var roundedAggregate = RoundAggregateRating(aggregate);
        course.avaliacao = FormatAggregateRating(roundedAggregate);
        await _context.SaveChangesAsync();

        return new SuccessResponse<ReadCourseRatingDto>(
            true,
            200,
            "Avaliação salva com sucesso.",
            new ReadCourseRatingDto
            {
                codigo_curso = courseId,
                minha_nota = persistedRating.nota,
                meu_comentario = persistedRating.comentario,
                media_curso = roundedAggregate,
            });
    }

    private async Task<CourseRating> PersistCourseRatingAsync(int courseId, int requestingUserId, decimal nota, string? comentario, DateTime now)
    {
        var existingRating = await _context.tb_curso_avaliacao
            .FirstOrDefaultAsync(cr => cr.codigo_curso == courseId && cr.codigo_usuario == requestingUserId);

        CourseRating? pendingInsert = null;
        if (existingRating == null)
        {
            pendingInsert = new CourseRating
            {
                codigo_curso = courseId,
                codigo_usuario = requestingUserId,
                nota = nota,
                comentario = comentario,
                created_at = now,
                updated_at = now,
            };

            _context.tb_curso_avaliacao.Add(pendingInsert);
        }
        else
        {
            existingRating.nota = nota;
            existingRating.comentario = comentario;
            existingRating.updated_at = now;
        }

        try
        {
            await _context.SaveChangesAsync();
            return pendingInsert ?? existingRating!;
        }
        catch (DbUpdateException ex) when (pendingInsert != null && IsCourseRatingUniqueConstraintViolation(ex))
        {
            _context.Entry(pendingInsert).State = EntityState.Detached;

            var concurrentRating = await _context.tb_curso_avaliacao
                .FirstOrDefaultAsync(cr => cr.codigo_curso == courseId && cr.codigo_usuario == requestingUserId);

            if (concurrentRating == null)
            {
                throw;
            }

            concurrentRating.nota = nota;
            concurrentRating.comentario = comentario;
            concurrentRating.updated_at = now;
            await _context.SaveChangesAsync();
            return concurrentRating;
        }
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

    private async Task<bool> CanAccessCourseContentAsync(int courseId, int userId)
    {
        return await _context.tb_usuario_curso
            .AsNoTracking()
            .AnyAsync(uc => uc.codigo_curso == courseId && uc.codigo_usuario == userId)
            || await IsUserConnectedToCourseAsync(courseId, userId);
    }

    private async Task<bool> IsUserConnectedToCourseAsync(int courseId, int userId)
    {
        return await _context.tb_curso_conexao_usuario
            .AsNoTracking()
            .AnyAsync(connection =>
                connection.codigo_curso == courseId &&
                connection.codigo_usuario == userId &&
                connection.status == ConnectedStatus);
    }

    private async Task<HashSet<int>> GetConnectedCourseIdsAsync(int userId)
    {
        return (await _context.tb_curso_conexao_usuario
                .AsNoTracking()
                .Where(connection => connection.codigo_usuario == userId && connection.status == ConnectedStatus)
                .Select(connection => connection.codigo_curso)
                .ToListAsync())
            .ToHashSet();
    }

    private async Task<HashSet<int>> GetCompletedLearningPathItemIdsAsync(int courseId, int userId)
    {
        return (await _context.tb_curso_trilha_progresso_usuario
                .AsNoTracking()
                .Where(progress =>
                    progress.codigo_curso == courseId &&
                    progress.codigo_usuario == userId &&
                    progress.concluido)
                .Select(progress => progress.codigo_trilha_item)
                .ToListAsync())
            .ToHashSet();
    }

    private async Task<Dictionary<int, int>> GetProgressLookupAsync(int userId)
    {
        var totalByCourse = await _context.tb_curso_trilha
            .AsNoTracking()
            .GroupBy(item => item.codigo_curso)
            .Select(group => new { CourseId = group.Key, Total = group.Count() })
            .ToDictionaryAsync(item => item.CourseId, item => item.Total);

        var completedByCourse = await _context.tb_curso_trilha_progresso_usuario
            .AsNoTracking()
            .Where(progress => progress.codigo_usuario == userId && progress.concluido)
            .GroupBy(progress => progress.codigo_curso)
            .Select(group => new { CourseId = group.Key, Completed = group.Count() })
            .ToDictionaryAsync(item => item.CourseId, item => item.Completed);

        return totalByCourse.ToDictionary(
            item => item.Key,
            item => CalculateProgress(item.Value, completedByCourse.TryGetValue(item.Key, out var completed) ? completed : 0));
    }

    private async Task<CourseProgressDto> BuildProgressDtoAsync(int courseId, int userId)
    {
        var total = await _context.tb_curso_trilha
            .AsNoTracking()
            .CountAsync(item => item.codigo_curso == courseId);
        var completed = await _context.tb_curso_trilha_progresso_usuario
            .AsNoTracking()
            .CountAsync(item => item.codigo_curso == courseId && item.codigo_usuario == userId && item.concluido);

        return new CourseProgressDto
        {
            codigo_curso = courseId,
            total_itens = total,
            itens_concluidos = completed,
            percentual = CalculateProgress(total, completed),
        };
    }

    private static int CalculateProgress(int totalItems, int completedItems)
    {
        if (totalItems <= 0)
        {
            return 0;
        }

        return Math.Clamp((int)Math.Round((decimal)completedItems / totalItems * 100, MidpointRounding.AwayFromZero), 0, 100);
    }

    private static string FormatAggregateRating(decimal value)
    {
        return value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static decimal RoundAggregateRating(decimal value)
    {
        return Math.Round(value, 1, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeComment(string? comment)
    {
        var trimmed = comment?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsCourseRatingUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("IX_tb_curso_avaliacao_codigo_curso_codigo_usuario", StringComparison.OrdinalIgnoreCase)
            || message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
    }

    private static List<CreateCourseSectionDto> NormalizeSections(IEnumerable<CreateCourseSectionDto>? sections)
    {
        return sections?
            .Select((section, index) => new CreateCourseSectionDto
            {
                titulo = section.titulo?.Trim() ?? string.Empty,
                conteudo = section.conteudo?.Trim() ?? string.Empty,
                ordem = section.ordem > 0 ? section.ordem : index + 1,
            })
            .Where(section => !string.IsNullOrWhiteSpace(section.titulo) && !string.IsNullOrWhiteSpace(section.conteudo))
            .OrderBy(section => section.ordem)
            .ToList()
            ?? new List<CreateCourseSectionDto>();
    }

    private static List<CourseLearningPathItemDto> NormalizeLearningPath(IEnumerable<CourseLearningPathItemDto>? items)
    {
        return items?
            .Select((item, index) => new CourseLearningPathItemDto
            {
                titulo = item.titulo?.Trim() ?? string.Empty,
                tipo = NormalizeType(item.tipo, "reading"),
                concluido_padrao = item.concluido_padrao,
                arquivo_nome = NormalizeNullable(item.arquivo_nome),
                arquivo_path = NormalizeNullable(item.arquivo_path),
                arquivo_tipo = NormalizeNullable(item.arquivo_tipo),
                ordem = item.ordem > 0 ? item.ordem : index + 1,
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.titulo))
            .OrderBy(item => item.ordem)
            .ToList()
            ?? new List<CourseLearningPathItemDto>();
    }

    private static List<CourseForumTopicDto> NormalizeForumTopics(IEnumerable<CourseForumTopicDto>? topics)
    {
        return topics?
            .Select((topic, index) => new CourseForumTopicDto
            {
                autor = topic.autor?.Trim() ?? string.Empty,
                titulo = topic.titulo?.Trim() ?? string.Empty,
                resumo = topic.resumo?.Trim() ?? string.Empty,
                ordem = topic.ordem > 0 ? topic.ordem : index + 1,
            })
            .Where(topic => !string.IsNullOrWhiteSpace(topic.titulo) && !string.IsNullOrWhiteSpace(topic.resumo))
            .OrderBy(topic => topic.ordem)
            .ToList()
            ?? new List<CourseForumTopicDto>();
    }

    private static List<CourseAssessmentItemDto> NormalizeAssessmentItems(IEnumerable<CourseAssessmentItemDto>? items)
    {
        return items?
            .Select((item, index) => new CourseAssessmentItemDto
            {
                titulo = item.titulo?.Trim() ?? string.Empty,
                tipo = NormalizeType(item.tipo, "activity"),
                quantidade_questoes = item.quantidade_questoes is > 0 ? item.quantidade_questoes : null,
                duracao = string.IsNullOrWhiteSpace(item.duracao) ? null : item.duracao.Trim(),
                prazo = item.prazo,
                ordem = item.ordem > 0 ? item.ordem : index + 1,
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.titulo))
            .OrderBy(item => item.ordem)
            .ToList()
            ?? new List<CourseAssessmentItemDto>();
    }

    private static List<CourseCertificateDto> NormalizeCertificates(IEnumerable<CourseCertificateDto>? certificates)
    {
        return certificates?
            .Select((certificate, index) => new CourseCertificateDto
            {
                titulo = certificate.titulo?.Trim() ?? string.Empty,
                descricao = certificate.descricao?.Trim() ?? string.Empty,
                status = certificate.status?.Trim() ?? string.Empty,
                progresso_padrao = Math.Clamp(certificate.progresso_padrao, 0, 100),
                disponivel_padrao = certificate.disponivel_padrao,
                ordem = certificate.ordem > 0 ? certificate.ordem : index + 1,
            })
            .Where(certificate => !string.IsNullOrWhiteSpace(certificate.titulo) && !string.IsNullOrWhiteSpace(certificate.descricao))
            .OrderBy(certificate => certificate.ordem)
            .ToList()
            ?? new List<CourseCertificateDto>();
    }

    private static CourseTeacherContactDto? NormalizeTeacherContact(CourseTeacherContactDto? contact)
    {
        if (contact == null)
        {
            return null;
        }

        var name = contact.nome_professor?.Trim() ?? string.Empty;
        var subtitle = string.IsNullOrWhiteSpace(contact.subtitulo) ? "Professor do curso" : contact.subtitulo.Trim();
        var message = string.IsNullOrWhiteSpace(contact.mensagem_orientacao)
            ? "Envie uma mensagem diretamente para o professor do curso:"
            : contact.mensagem_orientacao.Trim();

        return string.IsNullOrWhiteSpace(name)
            ? null
            : new CourseTeacherContactDto
            {
                nome_professor = name,
                subtitulo = subtitle,
                mensagem_orientacao = message,
            };
    }

    private static string NormalizeType(string? value, string fallback)
    {
        var type = value?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(type) ? fallback : type;
    }

    private static string? NormalizeNullable(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static CourseUserNoteDto ToNoteDto(CourseUserNote note)
    {
        return new CourseUserNoteDto
        {
            codigo = note.codigo,
            codigo_curso = note.codigo_curso,
            codigo_usuario = note.codigo_usuario,
            titulo = note.titulo,
            conteudo = note.conteudo,
            created_at = note.created_at,
            updated_at = note.updated_at,
        };
    }
}
