using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_avaliacao")]
public class CourseRating
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public int codigo_usuario { get; set; }
    public decimal nota { get; set; }
    public string? comentario { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
    public User UserJoin { get; set; } = null!;
}
