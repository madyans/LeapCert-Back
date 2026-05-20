using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_avaliacao_item")]
public class CourseAssessmentItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string tipo { get; set; } = "activity";
    public int? quantidade_questoes { get; set; }
    public string? duracao { get; set; }
    public DateTime? prazo { get; set; }
    public int ordem { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
}
