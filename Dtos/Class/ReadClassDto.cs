namespace leapcert_back.Dtos.Class;

public class ReadClassDto
{
    public int codigo { get; set; }
    public string descricao { get; set; }
    public string? avaliacao { get; set; }
    public DateTime created_at { get; set; }
    public string nome { get; set; }
    public int? codigo_genero { get; set; }
    public string genero { get; set; }
}