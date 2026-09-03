namespace DayMendesStore.Domain.Entities;

public class VariacaoProduto : BaseEntity
{
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public string Tamanho { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
}
