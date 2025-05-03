using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Models;

namespace leapcert_back.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {}
    
    public DbSet<User> Usuario { get; set; }
    public DbSet<Modules> tb_modulos { get; set; }
    public DbSet<PermissionModules> tb_permissoes_modulo { get; set; }
    public DbSet<Profile> tb_perfil { get; set; }
    public DbSet<Class> tb_curso { get; set; }
    public DbSet<Gender> tb_genero { get; set; }
    
    //JOINS: 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // joins de curso: 
        modelBuilder.Entity<Class>()
            .HasOne(cl => cl.GenderJoin)
            .WithMany(g => g.ClassJoin)
            .HasForeignKey(cl => cl.genero)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }
}