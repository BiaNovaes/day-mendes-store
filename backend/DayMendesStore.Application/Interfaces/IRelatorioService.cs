using DayMendesStore.Application.DTOs;

namespace DayMendesStore.Application.Interfaces;

public interface IRelatorioService
{
    Task<ResumoGeralDashboardDto> GetResumoGeralAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<List<ProdutoDesempenhoDto>> GetProdutosMaisVendidosAsync(RelatorioFiltroDto filtro, int top = 10, CancellationToken cancellationToken = default);
    Task<List<ProdutoDesempenhoDto>> GetProdutosMenosVendidosAsync(RelatorioFiltroDto filtro, int top = 10, CancellationToken cancellationToken = default);
    Task<List<ProdutoSemVendaDto>> GetProdutosParadosAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<List<ProdutoEstoqueBaixoDto>> GetProdutosEstoqueBaixoAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<List<CategoriaDesempenhoDto>> GetCategoriasMaisVendidasAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<List<TamanhoDesempenhoDto>> GetAnaliseTamanhosAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<List<ClienteDesempenhoDto>> GetMelhoresClientesAsync(RelatorioFiltroDto filtro, int top = 10, CancellationToken cancellationToken = default);
    Task<List<SugestaoReposicaoItemDto>> GetSugestoesReposicaoAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<RelatorioCompletoDto> GetRelatorioCompletoAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
}
