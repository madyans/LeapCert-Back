using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Models;

public class Profile
{
    [Key]
    public int codigo { get; set; }
    public string nome { get; set; }
}