using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.DTOs;

public class LojaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? CorPrimaria { get; set; }
    public string? CorSecundaria { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public LojaDto Loja { get; set; } = new();
}

public class LojaUpdateRequestDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? CorPrimaria { get; set; }
    public string? CorSecundaria { get; set; }
}

public class AlterarSenhaDto
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}

public class SetupLojaDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? CorPrimaria { get; set; } = "#E91E63";
    public string? CorSecundaria { get; set; } = "#9C27B0";
}
