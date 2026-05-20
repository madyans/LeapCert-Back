namespace leapcert_back.Dtos.Class;

public class ReadCourseRatingDto
{
    public int codigo_curso { get; set; }
    public decimal minha_nota { get; set; }
    public string? meu_comentario { get; set; }
    public decimal media_curso { get; set; }
}
