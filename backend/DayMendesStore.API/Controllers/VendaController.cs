using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class VendaController : ApiBaseController
{
    private readonly IVendaService _vendaService;

    public VendaController(IVendaService vendaService)
    {
        _vendaService = vendaService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<VendaDto>>> GetPaged([FromQuery] VendaFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _vendaService.GetPagedAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vendaService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<VendaDto>> Create([FromBody] VendaCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _vendaService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VendaDto>> Update(int id, [FromBody] VendaUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _vendaService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/finalizar")]
    public async Task<ActionResult<VendaDto>> Finalizar(int id, CancellationToken cancellationToken)
    {
        var result = await _vendaService.FinalizarAsync(id, cancellationToken);
        return Ok(result);
    }

    public record CancelarVendaRequest(string? Motivo);

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<VendaDto>> Cancelar(int id, [FromBody] CancelarVendaRequest? request, CancellationToken cancellationToken)
    {
        var result = await _vendaService.CancelarAsync(id, request?.Motivo, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}/rascunho")]
    public async Task<IActionResult> DeleteRascunho(int id, CancellationToken cancellationToken)
    {
        await _vendaService.DeleteRascunhoAsync(id, cancellationToken);
        return Ok(new { message = "Rascunho de venda excluído com sucesso." });
    }
}
