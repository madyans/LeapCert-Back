namespace leapcert_back.Interfaces;

public interface IResponses
{
    bool Flag { get; }
    int StatusCode { get; }
    string Message { get; }
}