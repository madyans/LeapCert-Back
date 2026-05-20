using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Dtos.Class;

public class CreateCourseSectionDto
{
    [Required]
    public string titulo { get; set; } = string.Empty;

    [Required]
    public string conteudo { get; set; } = string.Empty;

    public int ordem { get; set; }
}
