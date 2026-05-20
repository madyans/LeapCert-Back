using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_anotacao_usuario")]
public class CourseUserNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public int codigo_usuario { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string conteudo { get; set; } = string.Empty;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
    public User UserJoin { get; set; } = null!;
}
