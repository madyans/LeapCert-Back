using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_conexao_usuario")]
public class CourseConnection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_usuario { get; set; }
    public int codigo_curso { get; set; }
    public int codigo_criador_curso { get; set; }
    public string status { get; set; } = "connected";
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public User UserJoin { get; set; } = null!;
    public User CreatorJoin { get; set; } = null!;
    public Class ClassJoin { get; set; } = null!;
}
