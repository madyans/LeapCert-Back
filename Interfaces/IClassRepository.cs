namespace leapcert_back.Interfaces;

public interface IClassRepository
{
    Task<IResponses> GetAllAsync();
    Task<IResponses> GetByIdAsync(int id, int requestingUserId);
    Task<IResponses> GetTeacherByClass(int id);
}