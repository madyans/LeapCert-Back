namespace leapcert_back.Dtos.Users;

public record ReadUserSessionDTO(string codigo, string usuario, string nome, int perfil, string timeStamp, string Token);