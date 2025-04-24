using leapcert_back.Interfaces;
using leapcert_back.Models;
using Microsoft.EntityFrameworkCore;
using wsapi.Context;

namespace leapcert_back.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public Task<List<User>> GetAllAsync()
    {
        return _context.Usuario.ToListAsync();
    }
}