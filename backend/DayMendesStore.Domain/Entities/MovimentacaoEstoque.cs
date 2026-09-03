namespace DayMendesStore.Domain.Entities;

using DayMendesStore.Domain.Enums;

public class MovimentacaoEstoque : BaseEntity
{
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public int VariacaoProdutoId { get; set; }
    public VariacaoProduto? VariacaoProduto { get; set; }
    public TipoMovimentacaoEstoque Tipo { get; set; }
    public int Quantidade { get; set; }
    public string? Motivo { get; set; }
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
}
