using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using leapcert_back.Models;

namespace wsapi.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<Usuario> Usuario { get; set; }
}