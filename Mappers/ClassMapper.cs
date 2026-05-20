using leapcert_back.Dtos.Class;
using leapcert_back.Models;

namespace leapcert_back.Mappers;

public static class ClassMapper
{
    private const string DefaultCourseContentDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
    public static ReadClassDto ToReadClassDto(this UserClass dto, decimal? minhaNota = null, string? meuComentario = null)
    {
        var contentDescription = string.IsNullOrWhiteSpace(dto.ClassJoin.descricao)
            ? DefaultCourseContentDescription
            : dto.ClassJoin.descricao;
        var sections = dto.ClassJoin.SectionsJoin
            .OrderBy(section => section.ordem)
            .ThenBy(section => section.codigo)
            .Select(section => new ReadCourseSectionDto
            {
                codigo = section.codigo,
                titulo = section.titulo,
                conteudo = section.conteudo,
                ordem = section.ordem,
            })
            .ToList();

        var learningPath = dto.ClassJoin.LearningPathJoin
            .OrderBy(item => item.ordem)
            .ThenBy(item => item.codigo)
            .Select(item => new CourseLearningPathItemDto
            {
                codigo = item.codigo,
                titulo = item.titulo,
                tipo = item.tipo,
                concluido_padrao = item.concluido_padrao,
                arquivo_nome = item.arquivo_nome,
                arquivo_path = item.arquivo_path,
                arquivo_tipo = item.arquivo_tipo,
                ordem = item.ordem,
            })
            .ToList();

        var forumTopics = dto.ClassJoin.ForumTopicsJoin
            .OrderBy(topic => topic.ordem)
            .ThenBy(topic => topic.codigo)
            .Select(topic => new CourseForumTopicDto
            {
                codigo = topic.codigo,
                autor = topic.autor,
                titulo = topic.titulo,
                resumo = topic.resumo,
                ordem = topic.ordem,
            })
            .ToList();

        var assessmentItems = dto.ClassJoin.AssessmentItemsJoin
            .OrderBy(item => item.ordem)
            .ThenBy(item => item.codigo)
            .Select(item => new CourseAssessmentItemDto
            {
                codigo = item.codigo,
                titulo = item.titulo,
                tipo = item.tipo,
                quantidade_questoes = item.quantidade_questoes,
                duracao = item.duracao,
                prazo = item.prazo,
                ordem = item.ordem,
            })
            .ToList();

        var certificates = dto.ClassJoin.CertificatesJoin
            .OrderBy(certificate => certificate.ordem)
            .ThenBy(certificate => certificate.codigo)
            .Select(certificate => new CourseCertificateDto
            {
                codigo = certificate.codigo,
                titulo = certificate.titulo,
                descricao = certificate.descricao,
                status = certificate.status,
                progresso_padrao = certificate.progresso_padrao,
                disponivel_padrao = certificate.disponivel_padrao,
                ordem = certificate.ordem,
            })
            .ToList();

        var contact = dto.ClassJoin.TeacherContactJoin == null
            ? null
            : new CourseTeacherContactDto
            {
                codigo = dto.ClassJoin.TeacherContactJoin.codigo,
                nome_professor = dto.ClassJoin.TeacherContactJoin.nome_professor,
                subtitulo = dto.ClassJoin.TeacherContactJoin.subtitulo,
                mensagem_orientacao = dto.ClassJoin.TeacherContactJoin.mensagem_orientacao,
            };

        return new ReadClassDto()
        {
            codigo = dto.ClassJoin.codigo,
            codigo_professor = dto.codigo_usuario,
            nome = dto.ClassJoin.nome,
            descricao = contentDescription,
            avaliacao = dto.ClassJoin.avaliacao ?? "0.0",
            created_at = dto.ClassJoin.created_at,
            codigo_genero = dto.ClassJoin.genero,
            genero = dto.ClassJoin.GenderJoin.nome,
            path = dto.ClassJoin.PathJoin != null ? dto.ClassJoin.PathJoin.path : null,
            conteudo_descricao = contentDescription,
            instrutor_resumo = contentDescription,
            minha_nota = minhaNota,
            meu_comentario = meuComentario,
            secoes = sections,
            trilha = learningPath,
            forum_topicos = forumTopics,
            avaliacoes_itens = assessmentItems,
            certificados = certificates,
            contato_professor = contact,
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

    public static ReadClassCatalogDto ToCatalogDto(this Class course, int codigoProfessor)
    {
        return new ReadClassCatalogDto
        {
            codigo = course.codigo,
            codigo_professor = codigoProfessor,
            nome = course.nome,
            descricao = course.descricao,
            avaliacao = course.avaliacao ?? "0.0",
            created_at = course.created_at,
            codigo_genero = course.genero,
            genero = course.GenderJoin?.nome ?? "",
            path = course.PathJoin?.path,
        };
    }
}
