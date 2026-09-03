using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.DTOs;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Status Status { get; set; }
    public int TotalProdutos { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CategoriaCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public class CategoriaUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
