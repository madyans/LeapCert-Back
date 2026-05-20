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

public class TeacherRepositoryTests
{
    [Fact]
    public async Task CreateClass_PersistsCourseSectionsInOrder()
    {
        using var connection = BuildSqliteConnection();
        await using var context = BuildContext(connection);
        context.Database.EnsureCreated();
        SeedUserAndGender(context);
        var repository = new TeacherRepository(context, new FakeMinIoRepository());

        var result = await repository.CreateClass(new CreateClassDto
        {
            codigo_professor = 1,
            nome = "Curso com seções",
            descricao = "Resumo do curso",
            genero = 1,
            secoes = new List<CreateCourseSectionDto>
            {
                new() { titulo = "Objetivo", conteudo = "Aprender o fluxo principal.", ordem = 2 },
                new() { titulo = "Pré-requisitos", conteudo = "Conhecimento básico da área.", ordem = 1 },
            },
        });

        var success = Assert.IsType<SuccessResponse<CreateClassResultDto>>(result);
        var sections = context.tb_curso_secao
            .Where(section => section.codigo_curso == success.Data.codigo_curso)
            .OrderBy(section => section.ordem)
            .ToList();

        Assert.Equal(2, sections.Count);
        Assert.Equal("Pré-requisitos", sections[0].titulo);
        Assert.Equal(1, sections[0].ordem);
        Assert.Equal("Objetivo", sections[1].titulo);
        Assert.Equal(2, sections[1].ordem);
    }

    private static SqliteConnection BuildSqliteConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static ApplicationDbContext BuildContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedUserAndGender(ApplicationDbContext context)
    {
        context.Usuario.Add(new User
        {
            codigo = 1,
            nome = "Teacher",
            usuario = "teacher",
            email = "teacher@test.dev",
            senha = "123",
            created_at = DateTime.UtcNow,
        });
        context.tb_genero.Add(new Gender { codigo = 1, nome = "Tecnologia" });
        context.SaveChanges();
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
}
