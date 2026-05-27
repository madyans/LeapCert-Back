using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_trilha_progresso_usuario")]
public class CourseLearningPathProgress
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_usuario { get; set; }
    public int codigo_curso { get; set; }
    public int codigo_trilha_item { get; set; }
    public bool concluido { get; set; }
    public DateTime? concluido_em { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public User UserJoin { get; set; } = null!;
    public Class ClassJoin { get; set; } = null!;
    public CourseLearningPathItem LearningPathItemJoin { get; set; } = null!;
}
