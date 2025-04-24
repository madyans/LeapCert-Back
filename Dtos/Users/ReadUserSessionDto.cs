namespace leapcert_back.Dtos.Users;

public record ReadUserSessionDTO(string codigo, string usuario, string nome, string timeStamp, string Token);