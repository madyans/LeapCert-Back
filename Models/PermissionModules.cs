using System.ComponentModel.DataAnnotations;

namespace leapcert_back.Models;

public class PermissionModules
{
    [Key]
    public int codigo { get; set; }
    public int fk_modulo { get; set; }
    public int fk_perfil { get; set; }
}