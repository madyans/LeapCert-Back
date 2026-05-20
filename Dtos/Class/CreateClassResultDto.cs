namespace leapcert_back.Dtos.Class;

public class CreateClassResultDto
{
    public int codigo_curso { get; set; }
    public string nome { get; set; } = string.Empty;
    public string path { get; set; } = string.Empty;
}
