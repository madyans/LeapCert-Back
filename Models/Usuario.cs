using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Models;

public class Usuario
{
    [Key]
    public int codigo { get; set; }
    public string nome { get; set; }
    public string email { get; set; }
    public string usuario { get; set; }
    public string senha { get; set; }
    public Decimal avaliacao { get; set; }
    public DateTime created_at { get; set; }
    public int perfil { get; set; }
}