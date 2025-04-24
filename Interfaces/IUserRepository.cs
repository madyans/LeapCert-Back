using leapcert_back.Dtos.Users;
using leapcert_back.Models;

namespace leapcert_back.Interfaces;

public interface IUserRepository
{
    Task<IResponses> GetAllAsync();
    Task<IResponses> PostAsync(CreateUserDto user);
}