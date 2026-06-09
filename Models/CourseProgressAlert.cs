using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_alerta_progresso_usuario")]
public class CourseProgressAlert
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_usuario { get; set; }
    public int codigo_curso { get; set; }
    public int ultimo_percentual { get; set; }
    public DateTime ultima_evolucao_em { get; set; }
    public DateTime? ultima_exibicao_em { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public User UserJoin { get; set; } = null!;
    public Class ClassJoin { get; set; } = null!;
}
