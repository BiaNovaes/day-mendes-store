namespace DayMendesStore.Application.DTOs;

/// <summary>
/// Representa o saldo atual de estoque de um produto e suas variações.
/// </summary>
public class EstoqueProdutoDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public int EstoqueMinimo { get; set; }
    public int EstoqueTotal { get; set; }
    public List<EstoqueVariacaoDto> Variacoes { get; set; } = new();
}

/// <summary>
/// Representa o saldo atual disponível de uma variação física do produto.
/// </summary>
public class EstoqueVariacaoDto
{
    public int VariacaoId { get; set; }
    public int ProdutoId { get; set; }
    public string Tamanho { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
}

/// <summary>
/// DTO para atualização direta da quantidade atual disponível de uma variação.
/// </summary>
public class AtualizarEstoqueDto
{
    public int QuantidadeEstoque { get; set; }
}
