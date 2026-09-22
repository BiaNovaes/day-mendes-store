namespace DayMendesStore.Domain.Entities;

using DayMendesStore.Domain.Enums;

public class Venda : BaseEntity
{
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public StatusVenda StatusVenda { get; set; } = StatusVenda.Rascunho;
    public string? MotivoCancelamento { get; set; }

    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}