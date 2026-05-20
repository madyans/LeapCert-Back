using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_certificado")]
public class CourseCertificate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string descricao { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public int progresso_padrao { get; set; }
    public bool disponivel_padrao { get; set; }
    public int ordem { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
}
