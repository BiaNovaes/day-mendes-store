using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class ClienteController : ApiBaseController
{
    private readonly IClienteService _clienteService;

    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ClienteDto>>> GetPaged([FromQuery] ClienteFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _clienteService.GetPagedAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("ativos")]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> GetAllAtivos(CancellationToken cancellationToken)
    {
        var result = await _clienteService.GetAllAtivosAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _clienteService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] ClienteCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _clienteService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] ClienteUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _clienteService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancellationToken)
    {
        await _clienteService.InativarAsync(id, cancellationToken);
        return Ok(new { message = "Cliente inativado com sucesso." });
    }

    [HttpPatch("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id, CancellationToken cancellationToken)
    {
        await _clienteService.ReativarAsync(id, cancellationToken);
        return Ok(new { message = "Cliente reativado com sucesso." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _clienteService.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Cliente excluído com sucesso." });
    }
}
