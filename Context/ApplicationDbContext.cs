using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Models;

namespace wsapi.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {}
    
    public DbSet<User> Usuario { get; set; }
    public DbSet<Modules> tb_modulos { get; set; }
    public DbSet<PermissionModules> tb_permissoes_modulo { get; set; }
    public DbSet<Profile> tb_perfil { get; set; }
}