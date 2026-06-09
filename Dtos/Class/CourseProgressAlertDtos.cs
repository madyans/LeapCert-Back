namespace leapcert_back.Dtos.Class;

public class CourseProgressAlertDto
{
    public int codigo { get; set; }
    public int codigo_curso { get; set; }
    public string nome_curso { get; set; } = string.Empty;
    public int progresso_atual { get; set; }
    public int dias_sem_evolucao { get; set; }
    public DateTime ultima_evolucao_em { get; set; }
    public DateTime? ultima_exibicao_em { get; set; }
    public string mensagem { get; set; } = string.Empty;
}
