using leapcert_back.Context;
using leapcert_back.Dtos.Class;
using leapcert_back.Dtos.MinIo;
using leapcert_back.Interfaces;
using leapcert_back.Models;
using leapcert_back.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static leapcert_back.Responses.ResponseFactory;

namespace LeapCert_Back.Tests;

public class ClassRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsCourseSummary_WhenUserIsNotConnected()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        var repository = BuildRepository(context);

        var result = await repository.GetByIdAsync(10, requestingUserId: 2);

        var success = Assert.IsType<SuccessResponse<ReadClassDto>>(result);
        Assert.False(success.Data.can_access_content);
        Assert.False(success.Data.is_connected);
        Assert.Equal("available", success.Data.connection_status);
        Assert.Null(success.Data.path);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCourse_WhenUserOwnsAnotherCourse()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        var repository = BuildRepository(context);

        var result = await repository.GetByIdAsync(10, requestingUserId: 2);

        var success = Assert.IsType<SuccessResponse<ReadClassDto>>(result);
        Assert.Equal(10, success.Data.codigo);
        Assert.Equal("0.0", success.Data.avaliacao);
        Assert.NotNull(success.Data.conteudo_descricao);
        Assert.NotNull(success.Data.instrutor_resumo);
        Assert.Null(success.Data.minha_nota);
        Assert.Null(success.Data.meu_comentario);
    }

    [Fact]
    public async Task GetByIdAsync_UsesFallbackDescription_WhenCourseDescriptionIsBlank()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1, description: "   ");
        var repository = BuildRepository(context);

        var result = await repository.GetByIdAsync(10, requestingUserId: 1);

        var success = Assert.IsType<SuccessResponse<ReadClassDto>>(result);
        Assert.Equal(success.Data.conteudo_descricao, success.Data.descricao);
        Assert.NotEqual("   ", success.Data.descricao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_CreatesFirstRatingAndUpdatesAggregate()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 4,
            comentario = "Muito bom"
        });

        var success = Assert.IsType<SuccessResponse<ReadCourseRatingDto>>(result);
        Assert.Equal(10, success.Data.codigo_curso);
        Assert.Equal(4.0m, success.Data.media_curso);
        Assert.Equal(4, success.Data.minha_nota);
        Assert.Equal("Muito bom", success.Data.meu_comentario);

        var storedRating = Assert.Single(context.tb_curso_avaliacao);
        Assert.Equal(10, storedRating.codigo_curso);
        Assert.Equal(2, storedRating.codigo_usuario);
        Assert.Equal(4, storedRating.nota);
        Assert.Equal("4.0", context.tb_curso.Single(c => c.codigo == 10).avaliacao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_UpdatesExistingRatingForSameUser()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        context.tb_curso_avaliacao.Add(new CourseRating
        {
            codigo_curso = 10,
            codigo_usuario = 2,
            nota = 2,
            comentario = "Inicial",
            created_at = DateTime.UtcNow.AddMinutes(-5),
            updated_at = DateTime.UtcNow.AddMinutes(-5),
        });
        context.tb_curso.Single(c => c.codigo == 10).avaliacao = "2.0";
        context.SaveChanges();
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 5,
            comentario = "  Atualizado  "
        });

        var success = Assert.IsType<SuccessResponse<ReadCourseRatingDto>>(result);
        Assert.Equal(10, success.Data.codigo_curso);
        Assert.Equal(5.0m, success.Data.media_curso);
        Assert.Equal(5, success.Data.minha_nota);
        Assert.Equal("Atualizado", success.Data.meu_comentario);

        var storedRating = Assert.Single(context.tb_curso_avaliacao);
        Assert.Equal(5, storedRating.nota);
        Assert.Equal("Atualizado", storedRating.comentario);
        Assert.Equal("5.0", context.tb_curso.Single(c => c.codigo == 10).avaliacao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_RecalculatesAggregateAcrossUsers()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        SeedUser(context, userId: 3, name: "Reviewer 3");
        context.tb_curso_avaliacao.Add(new CourseRating
        {
            codigo_curso = 10,
            codigo_usuario = 3,
            nota = 5,
            comentario = "Excelente",
            created_at = DateTime.UtcNow.AddMinutes(-10),
            updated_at = DateTime.UtcNow.AddMinutes(-10),
        });
        context.tb_curso.Single(c => c.codigo == 10).avaliacao = "5.0";
        context.SaveChanges();
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 4,
            comentario = "Consistente"
        });

        var success = Assert.IsType<SuccessResponse<ReadCourseRatingDto>>(result);
        Assert.Equal(10, success.Data.codigo_curso);
        Assert.Equal(4.5m, success.Data.media_curso);
        Assert.Equal(4, success.Data.minha_nota);
        Assert.Equal(2, context.tb_curso_avaliacao.Count());
        Assert.Equal("4.5", context.tb_curso.Single(c => c.codigo == 10).avaliacao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_ReturnsRoundedAggregateMatchingStoredDisplayValue()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        SeedUser(context, userId: 3, name: "Reviewer 3");
        context.tb_curso_avaliacao.Add(new CourseRating
        {
            codigo_curso = 10,
            codigo_usuario = 3,
            nota = 5,
            comentario = "Excelente",
            created_at = DateTime.UtcNow.AddMinutes(-10),
            updated_at = DateTime.UtcNow.AddMinutes(-10),
        });
        context.SaveChanges();
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 3.5m,
            comentario = "Bom"
        });

        var success = Assert.IsType<SuccessResponse<ReadCourseRatingDto>>(result);
        Assert.Equal(4.3m, success.Data.media_curso);
        Assert.Equal("4.3", context.tb_curso.Single(c => c.codigo == 10).avaliacao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_NormalizesBlankCommentToNull()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 3,
            comentario = "   "
        });

        var success = Assert.IsType<SuccessResponse<ReadCourseRatingDto>>(result);
        Assert.Equal(10, success.Data.codigo_curso);
        Assert.Equal(3.0m, success.Data.media_curso);
        Assert.Null(success.Data.meu_comentario);

        var storedRating = Assert.Single(context.tb_curso_avaliacao);
        Assert.Null(storedRating.comentario);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_ReturnsNotFound_WhenCourseDoesNotExist()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(999, 2, new UpsertCourseRatingDto
        {
            nota = 4,
        });

        var notFound = Assert.IsType<ErrorResponse>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_ReturnsBadRequest_WhenRatingIsOutOfRange()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseWithTeacher(context, courseId: 11, teacherId: 2);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1);
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 0,
        });

        var badRequest = Assert.IsType<ErrorResponse>(result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Empty(context.tb_curso_avaliacao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_RollsBackRating_WhenAggregateRefreshFailsInsideTransaction()
    {
        using var connection = BuildSqliteConnection();
        using (var setupContext = BuildSqliteContext(connection))
        {
            setupContext.Database.EnsureCreated();
            SeedCourseWithTeacher(setupContext, courseId: 10, teacherId: 1);
            SeedCourseWithTeacher(setupContext, courseId: 11, teacherId: 2);
            SeedCourseConnection(setupContext, courseId: 10, userId: 2, creatorId: 1);
        }

        await using var context = BuildThrowOnSecondSaveSqliteContext(connection);
        var repository = BuildRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 4,
            comentario = "Muito bom"
        }));

        using var verificationContext = BuildSqliteContext(connection);
        Assert.Empty(verificationContext.tb_curso_avaliacao);
        Assert.Null(verificationContext.tb_curso.Single(c => c.codigo == 10).avaliacao);
    }

    [Fact]
    public async Task UpsertCourseRatingAsync_HandlesDuplicateSameUserSubmissionRace()
    {
        using var connection = BuildSqliteConnection();
        using (var setupContext = BuildSqliteContext(connection))
        {
            setupContext.Database.EnsureCreated();
            SeedCourseWithTeacher(setupContext, courseId: 10, teacherId: 1);
            SeedCourseWithTeacher(setupContext, courseId: 11, teacherId: 2);
            SeedCourseConnection(setupContext, courseId: 10, userId: 2, creatorId: 1);
        }

        await using var context = BuildDuplicateInsertOnFirstSaveSqliteContext(connection);
        var repository = BuildRepository(context);

        var result = await repository.UpsertCourseRatingAsync(10, 2, new UpsertCourseRatingDto
        {
            nota = 4,
            comentario = "  Atualizado pela corrida  "
        });

        var success = Assert.IsType<SuccessResponse<ReadCourseRatingDto>>(result);
        Assert.Equal(4.0m, success.Data.media_curso);
        Assert.Equal("Atualizado pela corrida", success.Data.meu_comentario);

        using var verificationContext = BuildSqliteContext(connection);
        var storedRating = Assert.Single(verificationContext.tb_curso_avaliacao);
        Assert.Equal(4, storedRating.nota);
        Assert.Equal("Atualizado pela corrida", storedRating.comentario);
        Assert.Equal("4.0", verificationContext.tb_curso.Single(c => c.codigo == 10).avaliacao);
    }

    [Fact]
    public async Task GetStudentProgressAlertsAsync_ReturnsAlert_WhenProgressDidNotIncreaseForFiveDays()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-6));
        SeedLearningPathItems(context, courseId: 10, count: 2);
        var repository = BuildRepository(context);

        var result = await repository.GetStudentProgressAlertsAsync(2);

        var success = Assert.IsType<SuccessResponse<List<CourseProgressAlertDto>>>(result);
        var alert = Assert.Single(success.Data);
        Assert.Equal(10, alert.codigo_curso);
        Assert.Equal(0, alert.progresso_atual);
        Assert.True(alert.dias_sem_evolucao >= 5);
        Assert.Contains("Curso 10", alert.mensagem);
    }

    [Fact]
    public async Task GetStudentProgressAlertsAsync_DoesNotReturnAlert_BeforeFiveDaysWithoutProgress()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-4));
        SeedLearningPathItems(context, courseId: 10, count: 2);
        var repository = BuildRepository(context);

        var result = await repository.GetStudentProgressAlertsAsync(2);

        var success = Assert.IsType<SuccessResponse<List<CourseProgressAlertDto>>>(result);
        Assert.Empty(success.Data);
    }

    [Fact]
    public async Task GetStudentProgressAlertsAsync_DoesNotReturnAlert_WhenCourseIsCompleted()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-6));
        var itemIds = SeedLearningPathItems(context, courseId: 10, count: 2);
        foreach (var itemId in itemIds)
        {
            SeedCompletedLearningPathProgress(context, courseId: 10, itemId: itemId, userId: 2);
        }
        var repository = BuildRepository(context);

        var result = await repository.GetStudentProgressAlertsAsync(2);

        var success = Assert.IsType<SuccessResponse<List<CourseProgressAlertDto>>>(result);
        Assert.Empty(success.Data);
    }

    [Fact]
    public async Task GetStudentProgressAlertsAsync_UsesRecentProgressAsInitialBaseline()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-10));
        var itemIds = SeedLearningPathItems(context, courseId: 10, count: 2);
        SeedCompletedLearningPathProgress(context, courseId: 10, itemId: itemIds[0], userId: 2, completedAt: DateTime.UtcNow.AddDays(-1));
        var repository = BuildRepository(context);

        var result = await repository.GetStudentProgressAlertsAsync(2);

        var success = Assert.IsType<SuccessResponse<List<CourseProgressAlertDto>>>(result);
        Assert.Empty(success.Data);
        var storedAlert = Assert.Single(context.tb_curso_alerta_progresso_usuario);
        Assert.Equal(50, storedAlert.ultimo_percentual);
        Assert.True(storedAlert.ultima_evolucao_em > DateTime.UtcNow.AddDays(-2));
    }


    [Fact]
    public async Task GetStudentProgressAlertsAsync_RestartsDelay_WhenProgressIncreases()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-10));
        var itemIds = SeedLearningPathItems(context, courseId: 10, count: 2);
        SeedCompletedLearningPathProgress(context, courseId: 10, itemId: itemIds[0], userId: 2);
        SeedProgressAlert(context, courseId: 10, userId: 2, lastPercent: 0, lastProgressAt: DateTime.UtcNow.AddDays(-8));
        var repository = BuildRepository(context);

        var result = await repository.GetStudentProgressAlertsAsync(2);

        var success = Assert.IsType<SuccessResponse<List<CourseProgressAlertDto>>>(result);
        Assert.Empty(success.Data);
        var storedAlert = Assert.Single(context.tb_curso_alerta_progresso_usuario);
        Assert.Equal(50, storedAlert.ultimo_percentual);
        Assert.True(storedAlert.ultima_evolucao_em > DateTime.UtcNow.AddMinutes(-1));
        Assert.Null(storedAlert.ultima_exibicao_em);
    }

    [Fact]
    public async Task GetStudentProgressAlertsAsync_RespectsLastSeenDelay()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-10));
        SeedLearningPathItems(context, courseId: 10, count: 2);
        SeedProgressAlert(
            context,
            courseId: 10,
            userId: 2,
            lastPercent: 0,
            lastProgressAt: DateTime.UtcNow.AddDays(-10),
            lastSeenAt: DateTime.UtcNow.AddDays(-2));
        var repository = BuildRepository(context);

        var result = await repository.GetStudentProgressAlertsAsync(2);

        var success = Assert.IsType<SuccessResponse<List<CourseProgressAlertDto>>>(result);
        Assert.Empty(success.Data);
    }

    [Fact]
    public async Task MarkStudentProgressAlertSeenAsync_UpdatesOnlyOwnedAlert()
    {
        using var context = BuildContext();
        SeedCourseWithTeacher(context, courseId: 10, teacherId: 1);
        SeedCourseConnection(context, courseId: 10, userId: 2, creatorId: 1, connectedAt: DateTime.UtcNow.AddDays(-10));
        SeedProgressAlert(context, courseId: 10, userId: 2, lastPercent: 0, lastProgressAt: DateTime.UtcNow.AddDays(-10));
        var alertId = context.tb_curso_alerta_progresso_usuario.Single().codigo;
        var repository = BuildRepository(context);

        var otherUserResult = await repository.MarkStudentProgressAlertSeenAsync(alertId, requestingUserId: 3);
        Assert.IsType<ErrorResponse>(otherUserResult);

        var result = await repository.MarkStudentProgressAlertSeenAsync(alertId, requestingUserId: 2);

        var success = Assert.IsType<SuccessResponse<CourseProgressAlertDto>>(result);
        Assert.Equal(alertId, success.Data.codigo);
        Assert.NotNull(context.tb_curso_alerta_progresso_usuario.Single().ultima_exibicao_em);
    }

    private static ApplicationDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext BuildThrowOnSecondSaveContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ThrowOnSecondSaveApplicationDbContext(options);
    }

    private static SqliteConnection BuildSqliteConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static ApplicationDbContext BuildSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext BuildThrowOnSecondSaveSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ThrowOnSecondSaveApplicationDbContext(options);
    }

    private static ApplicationDbContext BuildDuplicateInsertOnFirstSaveSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new DuplicateInsertOnFirstSaveApplicationDbContext(options);
    }

    private static ClassRepository BuildRepository(ApplicationDbContext context)
    {
        var minio = new FakeMinIoRepository();
        return new ClassRepository(context, minio);
    }

    private static void SeedCourseWithTeacher(ApplicationDbContext context, int courseId, int teacherId, string description = "Descricao")
    {
        SeedUser(context, teacherId, $"Teacher {teacherId}");
        SeedUser(context, 2, "Blocked");

        if (!context.tb_genero.Any(g => g.codigo == 1))
        {
            context.tb_genero.Add(new Gender { codigo = 1, nome = "Tecnologia" });
        }

        if (!context.tb_curso.Any(c => c.codigo == courseId))
        {
            context.tb_curso.Add(new Class
            {
                codigo = courseId,
                nome = $"Curso {courseId}",
                descricao = description,
                genero = 1,
                created_at = DateTime.UtcNow,
            });
            context.tb_curso_path.Add(new ClassPath { codigo_curso = courseId, path = $"curso-{courseId}/" });
        }

        if (!context.tb_usuario_curso.Any(uc => uc.codigo_curso == courseId && uc.codigo_usuario == teacherId))
        {
            context.tb_usuario_curso.Add(new UserClass
            {
                codigo_curso = courseId,
                codigo_usuario = teacherId,
                data_matricula = DateTime.UtcNow,
            });
        }

        context.SaveChanges();
    }

    private static void SeedCourseConnection(ApplicationDbContext context, int courseId, int userId, int creatorId, DateTime? connectedAt = null)
    {
        SeedUser(context, userId, $"Connected {userId}");
        SeedUser(context, creatorId, $"Teacher {creatorId}");

        if (!context.tb_curso_conexao_usuario.Any(connection => connection.codigo_curso == courseId && connection.codigo_usuario == userId))
        {
            var now = connectedAt ?? DateTime.UtcNow;
            context.tb_curso_conexao_usuario.Add(new CourseConnection
            {
                codigo_curso = courseId,
                codigo_usuario = userId,
                codigo_criador_curso = creatorId,
                status = "connected",
                created_at = now,
                updated_at = now,
            });
        }

        context.SaveChanges();
    }

    private static List<int> SeedLearningPathItems(ApplicationDbContext context, int courseId, int count)
    {
        var existingItems = context.tb_curso_trilha
            .Where(item => item.codigo_curso == courseId)
            .OrderBy(item => item.ordem)
            .Select(item => item.codigo)
            .ToList();

        if (existingItems.Count >= count)
        {
            return existingItems.Take(count).ToList();
        }

        var now = DateTime.UtcNow;
        for (var i = existingItems.Count; i < count; i++)
        {
            context.tb_curso_trilha.Add(new CourseLearningPathItem
            {
                codigo_curso = courseId,
                titulo = $"Etapa {i + 1}",
                tipo = "reading",
                concluido_padrao = false,
                ordem = i + 1,
                created_at = now,
                updated_at = now,
            });
        }

        context.SaveChanges();

        return context.tb_curso_trilha
            .Where(item => item.codigo_curso == courseId)
            .OrderBy(item => item.ordem)
            .Select(item => item.codigo)
            .ToList();
    }

    private static void SeedCompletedLearningPathProgress(ApplicationDbContext context, int courseId, int itemId, int userId, DateTime? completedAt = null)
    {
        var now = completedAt ?? DateTime.UtcNow;
        context.tb_curso_trilha_progresso_usuario.Add(new CourseLearningPathProgress
        {
            codigo_curso = courseId,
            codigo_usuario = userId,
            codigo_trilha_item = itemId,
            concluido = true,
            concluido_em = now,
            created_at = now,
            updated_at = now,
        });
        context.SaveChanges();
    }

    private static void SeedProgressAlert(
        ApplicationDbContext context,
        int courseId,
        int userId,
        int lastPercent,
        DateTime lastProgressAt,
        DateTime? lastSeenAt = null)
    {
        var now = DateTime.UtcNow;
        context.tb_curso_alerta_progresso_usuario.Add(new CourseProgressAlert
        {
            codigo_curso = courseId,
            codigo_usuario = userId,
            ultimo_percentual = lastPercent,
            ultima_evolucao_em = lastProgressAt,
            ultima_exibicao_em = lastSeenAt,
            created_at = now,
            updated_at = now,
        });
        context.SaveChanges();
    }

    private static void SeedUser(ApplicationDbContext context, int userId, string name)
    {
        if (context.Usuario.Local.Any(u => u.codigo == userId) || context.Usuario.Any(u => u.codigo == userId))
        {
            return;
        }

        context.Usuario.Add(new User
        {
            codigo = userId,
            nome = name,
            usuario = $"user{userId}",
            email = $"user{userId}@test.dev",
            senha = "123",
            created_at = DateTime.UtcNow,
        });
    }

    private sealed class FakeMinIoRepository : IMinIoRepository
    {
        public Task<IResponses> GetObject(GetObjectDto dto)
            => Task.FromResult<IResponses>(new SuccessResponse<string>(true, 200, "ok", "https://example.test/file"));

        public Task<IResponses> GetBucketItems(ListObjectsAsDto dto)
            => Task.FromResult<IResponses>(new SuccessResponse<List<BucketItemDto>>(true, 200, "ok", new List<BucketItemDto>()));

        public Task<IResponses> CreateFolder(string path, string folderName)
            => Task.FromResult<IResponses>(new SuccessResponse<string>(true, 200, "ok", folderName));

        public Task<IResponses> PutObject(UploadFileDto dto)
            => Task.FromResult<IResponses>(new SuccessResponse<string>(true, 200, "ok", dto.path));
    }

    private sealed class ThrowOnSecondSaveApplicationDbContext : ApplicationDbContext
    {
        private int _saveChangesAsyncCalls;

        public ThrowOnSecondSaveApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _saveChangesAsyncCalls++;
            if (_saveChangesAsyncCalls == 2)
            {
                throw new InvalidOperationException("Second save should not be required.");
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class DuplicateInsertOnFirstSaveApplicationDbContext : ApplicationDbContext
    {
        private bool _duplicateInjected;

        public DuplicateInsertOnFirstSaveApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!_duplicateInjected)
            {
                var pendingRating = ChangeTracker.Entries<CourseRating>()
                    .Where(entry => entry.State == EntityState.Added)
                    .Select(entry => entry.Entity)
                    .FirstOrDefault();

                if (pendingRating != null)
                {
                    _duplicateInjected = true;
                    await Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO tb_curso_avaliacao (codigo_curso, codigo_usuario, nota, comentario, created_at, updated_at)
                        VALUES ({pendingRating.codigo_curso}, {pendingRating.codigo_usuario}, {1m}, {"Corrida"}, {pendingRating.created_at}, {pendingRating.updated_at})", cancellationToken);

                    throw new DbUpdateException(
                        "Simulated unique constraint race.",
                        new Exception("UNIQUE constraint failed: tb_curso_avaliacao.codigo_curso, tb_curso_avaliacao.codigo_usuario"));
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
