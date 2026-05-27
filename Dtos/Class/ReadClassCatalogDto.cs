using leapcert_back.Dtos.MinIo;

namespace leapcert_back.Dtos.Class;

public class ReadClassCatalogDto
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
    public List<BucketItemDto> objects { get; set; } = new();
}
