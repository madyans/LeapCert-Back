using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Models;

public class Modules
{
    [Key]
    public int codigo { get; set; }
    public string nome { get; set; }
    public string rota { get; set; }
    public string icone { get; set; }
    public bool hasChildren { get; set; }
    public int? childoff { get; set; }
    public int? ordem { get; set; }
}