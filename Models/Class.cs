using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace leapcert_back.Models;

public class Class
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int codigo { get; set; }
    public string descricao { get; set; }
    public string? avaliacao { get; set; }
    public DateTime created_at { get; set; }
    public string nome { get; set; }
    public int? genero { get; set; }

    // joins
    public Gender GenderJoin { get; set; }
    public ClassPath PathJoin { get; set; }
}