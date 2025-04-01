namespace leapcert_back.Dtos.Users;

public class CreateUserDto
{
    public string nome { get; set; }
    public string email { get; set; }
    public string usuario { get; set; }
    public string senha { get; set; }
    public int perfil { get; set; }
}