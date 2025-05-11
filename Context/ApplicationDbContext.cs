using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace leapcert_back.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    { }

    public DbSet<User> Usuario { get; set; }
    public DbSet<Modules> tb_modulos { get; set; }
    public DbSet<PermissionModules> tb_permissoes_modulo { get; set; }
    public DbSet<Profile> tb_perfil { get; set; }
    public DbSet<Class> tb_curso { get; set; }
    public DbSet<Gender> tb_genero { get; set; }
    public DbSet<UserClass> tb_usuario_curso { get; set; }
    public DbSet<ClassPath> tb_curso_path { get; set; }

    //JOINS: 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // joins de curso: 
        modelBuilder.Entity<Class>()
            .HasOne(cl => cl.GenderJoin)
            .WithMany(g => g.ClassJoin)
            .HasForeignKey(cl => cl.genero)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Class>()
            .HasOne(cl => cl.PathJoin)
            .WithMany()
            .HasForeignKey(cl => cl.codigo)
            .OnDelete(DeleteBehavior.Cascade);

        // joins usuario_curso:
        modelBuilder.Entity<UserClass>()
            .HasOne(uc => uc.ClassJoin)
            .WithMany()
            .HasForeignKey(uc => uc.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserClass>()
            .HasOne(uc => uc.UserJoin)
            .WithMany()
            .HasForeignKey(uc => uc.codigo_usuario)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}