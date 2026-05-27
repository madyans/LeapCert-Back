namespace leapcert_back.Dtos.Class;

public class ReadClassDto
{
    public int codigo { get; set; }
    public int codigo_professor { get; set; }
    public string descricao { get; set; }
    public string? avaliacao { get; set; }
    public DateTime created_at { get; set; }
    public string nome { get; set; }
    public int? codigo_genero { get; set; }
    public string genero { get; set; }
    public string nome_professor { get; set; } = string.Empty;
    public string? path { get; set; }
    public string connection_status { get; set; } = "available";
    public bool is_owner { get; set; }
    public bool is_connected { get; set; }
    public bool can_access_content { get; set; }
    public int progresso_usuario { get; set; }
    public string conteudo_descricao { get; set; } = string.Empty;
    public string instrutor_resumo { get; set; } = string.Empty;
    public decimal? minha_nota { get; set; }
    public string? meu_comentario { get; set; }
    public List<ReadCourseSectionDto> secoes { get; set; } = new();
    public List<CourseLearningPathItemDto> trilha { get; set; } = new();
    public List<CourseForumTopicDto> forum_topicos { get; set; } = new();
    public List<CourseAssessmentItemDto> avaliacoes_itens { get; set; } = new();
    public List<CourseCertificateDto> certificados { get; set; } = new();
    public CourseTeacherContactDto? contato_professor { get; set; }
    public List<CourseUserNoteDto> anotacoes { get; set; } = new();
}
