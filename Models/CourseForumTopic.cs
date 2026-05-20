using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_forum_topico")]
public class CourseForumTopic
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public string autor { get; set; } = string.Empty;
    public string titulo { get; set; } = string.Empty;
    public string resumo { get; set; } = string.Empty;
    public int ordem { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
}
