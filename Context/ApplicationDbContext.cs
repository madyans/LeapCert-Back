using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<CourseRating> tb_curso_avaliacao { get; set; }
    public DbSet<CourseSection> tb_curso_secao { get; set; }
    public DbSet<CourseLearningPathItem> tb_curso_trilha { get; set; }
    public DbSet<CourseForumTopic> tb_curso_forum_topico { get; set; }
    public DbSet<CourseAssessmentItem> tb_curso_avaliacao_item { get; set; }
    public DbSet<CourseCertificate> tb_curso_certificado { get; set; }
    public DbSet<CourseTeacherContact> tb_curso_professor_contato { get; set; }
    public DbSet<CourseUserNote> tb_curso_anotacao_usuario { get; set; }
    public DbSet<CourseConnection> tb_curso_conexao_usuario { get; set; }
    public DbSet<CourseLearningPathProgress> tb_curso_trilha_progresso_usuario { get; set; }
    public DbSet<CourseProgressAlert> tb_curso_alerta_progresso_usuario { get; set; }

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
            .WithOne()
            .HasForeignKey<ClassPath>(cp => cp.codigo_curso)
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

        modelBuilder.Entity<CourseRating>()
            .HasIndex(cr => new { cr.codigo_curso, cr.codigo_usuario })
            .IsUnique();

        modelBuilder.Entity<CourseRating>()
            .Property(cr => cr.nota)
            .HasPrecision(4, 2);

        modelBuilder.Entity<CourseRating>()
            .HasOne(cr => cr.ClassJoin)
            .WithMany()
            .HasForeignKey(cr => cr.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseRating>()
            .HasOne(cr => cr.UserJoin)
            .WithMany()
            .HasForeignKey(cr => cr.codigo_usuario)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseSection>()
            .HasIndex(cs => new { cs.codigo_curso, cs.ordem });

        modelBuilder.Entity<CourseSection>()
            .HasOne(cs => cs.ClassJoin)
            .WithMany(c => c.SectionsJoin)
            .HasForeignKey(cs => cs.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseLearningPathItem>()
            .HasIndex(item => new { item.codigo_curso, item.ordem });

        modelBuilder.Entity<CourseLearningPathItem>()
            .HasOne(item => item.ClassJoin)
            .WithMany(c => c.LearningPathJoin)
            .HasForeignKey(item => item.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseForumTopic>()
            .HasIndex(topic => new { topic.codigo_curso, topic.ordem });

        modelBuilder.Entity<CourseForumTopic>()
            .HasOne(topic => topic.ClassJoin)
            .WithMany(c => c.ForumTopicsJoin)
            .HasForeignKey(topic => topic.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseAssessmentItem>()
            .HasIndex(item => new { item.codigo_curso, item.ordem });

        modelBuilder.Entity<CourseAssessmentItem>()
            .HasOne(item => item.ClassJoin)
            .WithMany(c => c.AssessmentItemsJoin)
            .HasForeignKey(item => item.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseCertificate>()
            .HasIndex(certificate => new { certificate.codigo_curso, certificate.ordem });

        modelBuilder.Entity<CourseCertificate>()
            .HasOne(certificate => certificate.ClassJoin)
            .WithMany(c => c.CertificatesJoin)
            .HasForeignKey(certificate => certificate.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseTeacherContact>()
            .HasIndex(contact => contact.codigo_curso)
            .IsUnique();

        modelBuilder.Entity<CourseTeacherContact>()
            .HasOne(contact => contact.ClassJoin)
            .WithOne(c => c.TeacherContactJoin)
            .HasForeignKey<CourseTeacherContact>(contact => contact.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseUserNote>()
            .HasIndex(note => new { note.codigo_curso, note.codigo_usuario, note.updated_at });

        modelBuilder.Entity<CourseUserNote>()
            .HasOne(note => note.ClassJoin)
            .WithMany()
            .HasForeignKey(note => note.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseUserNote>()
            .HasOne(note => note.UserJoin)
            .WithMany()
            .HasForeignKey(note => note.codigo_usuario)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseConnection>()
            .HasIndex(connection => new { connection.codigo_usuario, connection.codigo_curso })
            .IsUnique();

        modelBuilder.Entity<CourseConnection>()
            .HasIndex(connection => new { connection.codigo_curso, connection.status });

        modelBuilder.Entity<CourseConnection>()
            .HasOne(connection => connection.UserJoin)
            .WithMany()
            .HasForeignKey(connection => connection.codigo_usuario)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseConnection>()
            .HasOne(connection => connection.CreatorJoin)
            .WithMany()
            .HasForeignKey(connection => connection.codigo_criador_curso)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseConnection>()
            .HasOne(connection => connection.ClassJoin)
            .WithMany()
            .HasForeignKey(connection => connection.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseLearningPathProgress>()
            .HasIndex(progress => new { progress.codigo_usuario, progress.codigo_trilha_item })
            .IsUnique();

        modelBuilder.Entity<CourseLearningPathProgress>()
            .HasIndex(progress => new { progress.codigo_usuario, progress.codigo_curso });

        modelBuilder.Entity<CourseLearningPathProgress>()
            .HasOne(progress => progress.UserJoin)
            .WithMany()
            .HasForeignKey(progress => progress.codigo_usuario)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseLearningPathProgress>()
            .HasOne(progress => progress.ClassJoin)
            .WithMany()
            .HasForeignKey(progress => progress.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseLearningPathProgress>()
            .HasOne(progress => progress.LearningPathItemJoin)
            .WithMany()
            .HasForeignKey(progress => progress.codigo_trilha_item)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CourseProgressAlert>()
            .HasIndex(alert => new { alert.codigo_usuario, alert.codigo_curso })
            .IsUnique();

        modelBuilder.Entity<CourseProgressAlert>()
            .HasIndex(alert => new { alert.codigo_usuario, alert.ultima_evolucao_em });

        modelBuilder.Entity<CourseProgressAlert>()
            .HasOne(alert => alert.UserJoin)
            .WithMany()
            .HasForeignKey(alert => alert.codigo_usuario)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseProgressAlert>()
            .HasOne(alert => alert.ClassJoin)
            .WithMany()
            .HasForeignKey(alert => alert.codigo_curso)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
