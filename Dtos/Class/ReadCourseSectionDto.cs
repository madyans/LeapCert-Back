namespace leapcert_back.Dtos.Class;

public class ReadCourseSectionDto
{
    public int codigo { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string conteudo { get; set; } = string.Empty;
    public int ordem { get; set; }
}
