using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_trilha")]
public class CourseLearningPathItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string tipo { get; set; } = "reading";
    public bool concluido_padrao { get; set; }
    public string? arquivo_nome { get; set; }
    public string? arquivo_path { get; set; }
    public string? arquivo_tipo { get; set; }
    public int ordem { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
}
