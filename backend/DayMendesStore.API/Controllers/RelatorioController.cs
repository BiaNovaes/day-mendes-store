using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class RelatorioController : ApiBaseController
{
    private readonly IRelatorioService _relatorioService;
    private readonly IPdfReportService _pdfReportService;

    public RelatorioController(IRelatorioService relatorioService, IPdfReportService pdfReportService)
    {
        _relatorioService = relatorioService;
        _pdfReportService = pdfReportService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ResumoGeralDashboardDto>> GetDashboard([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetResumoGeralAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("produtos-mais-vendidos")]
    public async Task<ActionResult<List<ProdutoDesempenhoDto>>> GetProdutosMaisVendidos([FromQuery] RelatorioFiltroDto filtro, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _relatorioService.GetProdutosMaisVendidosAsync(filtro, top, cancellationToken);
        return Ok(result);
    }

    [HttpGet("produtos-menos-vendidos")]
    public async Task<ActionResult<List<ProdutoDesempenhoDto>>> GetProdutosMenosVendidos([FromQuery] RelatorioFiltroDto filtro, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _relatorioService.GetProdutosMenosVendidosAsync(filtro, top, cancellationToken);
        return Ok(result);
    }

    [HttpGet("produtos-parados")]
    public async Task<ActionResult<List<ProdutoSemVendaDto>>> GetProdutosParados([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetProdutosParadosAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("estoque-baixo")]
    public async Task<ActionResult<List<ProdutoEstoqueBaixoDto>>> GetProdutosEstoqueBaixo([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetProdutosEstoqueBaixoAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("categorias-mais-vendidas")]
    public async Task<ActionResult<List<CategoriaDesempenhoDto>>> GetCategoriasMaisVendidas([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetCategoriasMaisVendidasAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analise-tamanhos")]
    public async Task<ActionResult<List<TamanhoDesempenhoDto>>> GetAnaliseTamanhos([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetAnaliseTamanhosAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("melhores-clientes")]
    public async Task<ActionResult<List<ClienteDesempenhoDto>>> GetMelhoresClientes([FromQuery] RelatorioFiltroDto filtro, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _relatorioService.GetMelhoresClientesAsync(filtro, top, cancellationToken);
        return Ok(result);
    }

    [HttpGet("sugestao-reposicao")]
    public async Task<ActionResult<List<SugestaoReposicaoItemDto>>> GetSugestoesReposicao([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetSugestoesReposicaoAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("completo")]
    public async Task<ActionResult<RelatorioCompletoDto>> GetRelatorioCompleto([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _relatorioService.GetRelatorioCompletoAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> GetPdf([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken)
    {
        var bytes = await _pdfReportService.GerarRelatorioCompletoPdfAsync(filtro, cancellationToken);
        var fileName = $"relatorio_day_mendes_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}
