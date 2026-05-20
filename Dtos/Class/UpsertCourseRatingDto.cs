using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Dtos.Class;

public class UpsertCourseRatingDto
{
    [Range(1, 5)]
    public decimal nota { get; set; }
    public string? comentario { get; set; }
}
