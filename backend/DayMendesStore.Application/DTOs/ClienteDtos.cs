using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public Status Status { get; set; }
    public int TotalCompras { get; set; }
    public decimal ValorTotalComprado { get; set; }
    public DateTime? UltimaCompraEm { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ClienteCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
}

public class ClienteUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
}

public class ClienteFiltroDto : PaginationParamsDto
{
    public string? TermoBusca { get; set; }
    public Status? Status { get; set; }
}
