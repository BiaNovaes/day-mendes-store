using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.DTOs;

public class ItemVendaCreateDto
{
    public int ProdutoId { get; set; }
    public int VariacaoProdutoId { get; set; }
    public int Quantidade { get; set; }
}

public class ItemVendaDto
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public string? ProdutoFoto { get; set; }
    public int VariacaoProdutoId { get; set; }
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class VendaCreateDto
{
    public int? ClienteId { get; set; }
    public DateTime? DataVenda { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public bool FinalizarImediatamente { get; set; } = false;
    public List<ItemVendaCreateDto> Itens { get; set; } = new();
}

public class VendaUpdateDto
{
    public int? ClienteId { get; set; }
    public DateTime? DataVenda { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public List<ItemVendaCreateDto> Itens { get; set; } = new();
}

public class VendaDto
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public string? ClienteTelefone { get; set; }
    public DateTime DataVenda { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public StatusVenda StatusVenda { get; set; }
    public Status Status { get; set; }
    public int TotalItens => Itens.Sum(i => i.Quantidade);
    public List<ItemVendaDto> Itens { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VendaFiltroDto : PaginationParamsDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int? ClienteId { get; set; }
    public string? FormaPagamento { get; set; }
    public StatusVenda? StatusVenda { get; set; }
}
