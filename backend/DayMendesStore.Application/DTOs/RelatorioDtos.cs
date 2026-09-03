namespace DayMendesStore.Application.DTOs;

public class RelatorioFiltroDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int? CategoriaId { get; set; }
    public string? Tamanho { get; set; }
}

public class ResumoGeralDashboardDto
{
    public decimal TotalFaturado { get; set; }
    public int TotalVendas { get; set; }
    public int TotalProdutosVendidos { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal LucroBrutoEstimado { get; set; }
    public decimal MargemLucroMedia { get; set; }
    public List<VendaPeriodoItemDto> VendasPorPeriodo { get; set; } = new();
    public List<FormaPagamentoItemDto> VendasPorFormaPagamento { get; set; } = new();
}

public class VendaPeriodoItemDto
{
    public string Periodo { get; set; } = string.Empty; // ex: "2026-08-29" ou "08/2026"
    public int QuantidadeVendas { get; set; }
    public decimal TotalFaturado { get; set; }
}

public class FormaPagamentoItemDto
{
    public string FormaPagamento { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal Percentual { get; set; }
}

public class ProdutoDesempenhoDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public int QuantidadeVendida { get; set; }
    public decimal ValorTotalVendido { get; set; }
    public decimal LucroEstimado { get; set; }
    public int EstoqueAtual { get; set; }
}

public class ProdutoSemVendaDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public int EstoqueAtual { get; set; }
    public decimal ValorVenda { get; set; }
    public int DiasSemVenda { get; set; }
    public string ClassificacaoRotatividade { get; set; } = string.Empty; // "Parado", "Baixa Rotatividade", "Nunca Vendido"
}

public class ProdutoEstoqueBaixoDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public string StatusEstoque { get; set; } = string.Empty; // "Zerado" ou "Abaixo do Mínimo"
}

public class CategoriaDesempenhoDto
{
    public int CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeProdutosVendidos { get; set; }
    public decimal ValorTotalFaturado { get; set; }
    public decimal PercentualFaturamento { get; set; }
}

public class TamanhoDesempenhoDto
{
    public string Tamanho { get; set; } = string.Empty;
    public int QuantidadeVendida { get; set; }
    public decimal ValorTotal { get; set; }
    public int QuantidadeEmEstoque { get; set; }
    public int QuantidadeProdutosZerados { get; set; }
    public string VelocidadeSaida { get; set; } = string.Empty; // "Alta", "Média", "Baixa"
}

public class ClienteDesempenhoDto
{
    public int? ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public int TotalCompras { get; set; }
    public decimal ValorTotalComprado { get; set; }
    public decimal TicketMedio { get; set; }
    public DateTime? UltimaCompra { get; set; }
}

public class SugestaoReposicaoItemDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public int VendasUltimos30Dias { get; set; }
    public int QuantidadeSugerida { get; set; }
    public string JustificativaSugestao { get; set; } = string.Empty;
}

public class RelatorioCompletoDto
{
    public LojaDto Loja { get; set; } = new();
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
    public ResumoGeralDashboardDto ResumoGeral { get; set; } = new();
    public List<ProdutoDesempenhoDto> ProdutosMaisVendidos { get; set; } = new();
    public List<ProdutoDesempenhoDto> ProdutosMenosVendidos { get; set; } = new();
    public List<ProdutoSemVendaDto> ProdutosParados { get; set; } = new();
    public List<ProdutoEstoqueBaixoDto> ProdutosEstoqueBaixo { get; set; } = new();
    public List<CategoriaDesempenhoDto> CategoriasMaisVendidas { get; set; } = new();
    public List<TamanhoDesempenhoDto> AnaliseTamanhos { get; set; } = new();
    public List<ClienteDesempenhoDto> MelhoresClientes { get; set; } = new();
    public List<SugestaoReposicaoItemDto> SugestoesReposicao { get; set; } = new();
}
