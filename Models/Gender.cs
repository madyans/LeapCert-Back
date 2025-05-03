using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Models;

public class Gender
{
    [Key]
    public int codigo { get; set; }
    public string nome { get; set; }
    
    // joins
    public ICollection<Class> ClassJoin { get; set; }}