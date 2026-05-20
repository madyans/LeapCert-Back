using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

[Table("tb_curso_professor_contato")]
public class CourseTeacherContact
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public string nome_professor { get; set; } = string.Empty;
    public string subtitulo { get; set; } = "Professor do curso";
    public string mensagem_orientacao { get; set; } = "Envie uma mensagem diretamente para o professor do curso:";
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public Class ClassJoin { get; set; } = null!;
}
