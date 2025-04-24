using leapcert_back.Interfaces;

namespace leapcert_back.Responses;

public class ResponseFactory
{
    public record SuccessResponse(bool Flag, int StatusCode, string Message) : IResponses;
    public record SuccessResponse<T>(bool Flag, int StatusCode, string Message,  T Data) : IResponses;
    public record ErrorResponse(bool Flag, int StatusCode, string Message) : IResponses;
}