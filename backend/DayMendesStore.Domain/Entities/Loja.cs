namespace DayMendesStore.Domain.Entities;

public class Loja : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty; // Hash
    public string? Cnpj { get; set; }
    public string? CorPrimaria { get; set; }
    public string? CorSecundaria { get; set; }
}