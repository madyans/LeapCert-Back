using leapcert_back.Models;

namespace leapcert_back.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
}