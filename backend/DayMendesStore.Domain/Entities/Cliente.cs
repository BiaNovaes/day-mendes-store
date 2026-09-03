namespace DayMendesStore.Domain.Entities;

public class Cliente : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
}