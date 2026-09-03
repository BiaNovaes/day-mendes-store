namespace DayMendesStore.Domain.Entities;

public class ItemVenda : BaseEntity
{
    public int VendaId { get; set; }
    public Venda? Venda { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public int VariacaoProdutoId { get; set; }
    public VariacaoProduto? VariacaoProduto { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}