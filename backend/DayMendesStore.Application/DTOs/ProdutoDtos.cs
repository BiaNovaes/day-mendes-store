using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.DTOs;

public class VariacaoProdutoDto
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string Tamanho { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VariacaoProdutoCreateDto
{
    public string Tamanho { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
}

public class VariacaoProdutoUpdateDto
{
    public int? Id { get; set; }
    public string Tamanho { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
    public Status? Status { get; set; }
}

public class ProdutoDto
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Marca { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVenda { get; set; }
    public decimal MargemLucro => ValorCompra > 0 ? Math.Round(((ValorVenda - ValorCompra) / ValorCompra) * 100, 2) : 0;
    public decimal LucroUnitario => ValorVenda - ValorCompra;
    public int QuantidadeEstoque => EstoqueTotal;
    public int EstoqueTotal => Variacoes.Where(v => v.Status == Status.Ativo).Sum(v => v.QuantidadeEstoque);
    public int EstoqueMinimo { get; set; }
    public bool EstoqueBaixo => QuantidadeEstoque <= EstoqueMinimo;
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<VariacaoProdutoDto> Variacoes { get; set; } = new();
}

public class ProdutoCreateDto
{
    public int CategoriaId { get; set; }
    public string? Foto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Marca { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVenda { get; set; }
    public int EstoqueMinimo { get; set; }
    public List<VariacaoProdutoCreateDto>? Variacoes { get; set; } = new();
}

public class ProdutoUpdateDto
{
    public int CategoriaId { get; set; }
    public string? Foto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Marca { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVenda { get; set; }
    public int EstoqueMinimo { get; set; }
    public List<VariacaoProdutoUpdateDto>? Variacoes { get; set; }
}

public class ProdutoFiltroDto : PaginationParamsDto
{
    public string? TermoBusca { get; set; }
    public int? CategoriaId { get; set; }
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public Status? Status { get; set; }
    public bool? ApenasEmEstoque { get; set; }
    public bool? ApenasEstoqueBaixo { get; set; }
}
