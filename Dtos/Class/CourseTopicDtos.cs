namespace leapcert_back.Dtos.Class;

public class CourseLearningPathItemDto
{
    public int codigo { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string tipo { get; set; } = "reading";
    public bool concluido_padrao { get; set; }
    public string? arquivo_nome { get; set; }
    public string? arquivo_path { get; set; }
    public string? arquivo_tipo { get; set; }
    public int ordem { get; set; }
}

public class CourseForumTopicDto
{
    public int codigo { get; set; }
    public string autor { get; set; } = string.Empty;
    public string titulo { get; set; } = string.Empty;
    public string resumo { get; set; } = string.Empty;
    public int ordem { get; set; }
}

public class CourseAssessmentItemDto
{
    public int codigo { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string tipo { get; set; } = "activity";
    public int? quantidade_questoes { get; set; }
    public string? duracao { get; set; }
    public DateTime? prazo { get; set; }
    public int ordem { get; set; }
}

public class CourseCertificateDto
{
    public int codigo { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string descricao { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public int progresso_padrao { get; set; }
    public bool disponivel_padrao { get; set; }
    public int ordem { get; set; }
}

public class CourseTeacherContactDto
{
    public int codigo { get; set; }
    public string nome_professor { get; set; } = string.Empty;
    public string subtitulo { get; set; } = "Professor do curso";
    public string mensagem_orientacao { get; set; } = "Envie uma mensagem diretamente para o professor do curso:";
}

public class CourseTopicsDto
{
    public List<CreateCourseSectionDto> secoes { get; set; } = new();
    public List<CourseLearningPathItemDto> trilha { get; set; } = new();
    public List<CourseForumTopicDto> forum_topicos { get; set; } = new();
    public List<CourseAssessmentItemDto> avaliacoes_itens { get; set; } = new();
    public List<CourseCertificateDto> certificados { get; set; } = new();
    public CourseTeacherContactDto? contato_professor { get; set; }
}

public class CourseUserNoteDto
{
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public int codigo_usuario { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string conteudo { get; set; } = string.Empty;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

public class UpsertCourseUserNoteDto
{
    public string titulo { get; set; } = string.Empty;
    public string conteudo { get; set; } = string.Empty;
}

public class CreateCourseForumTopicDto
{
    public string titulo { get; set; } = string.Empty;
    public string resumo { get; set; } = string.Empty;
}
