namespace leapcert_back.Interfaces;

public interface IModulesRepository
{
    Task<IResponses> GetAllAsync();
}