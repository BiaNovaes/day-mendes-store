namespace DayMendesStore.Domain.Entities;

public class Produto : BaseEntity
{
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public string? Foto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Marca { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVenda { get; set; }
    public int EstoqueMinimo { get; set; } = 0;

    public ICollection<VariacaoProduto> Variacoes { get; set; } = new List<VariacaoProduto>();
}