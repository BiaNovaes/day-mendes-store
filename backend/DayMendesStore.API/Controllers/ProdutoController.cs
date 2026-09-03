using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class ProdutoController : ApiBaseController
{
    private readonly IProdutoService _produtoService;

    public ProdutoController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProdutoDto>>> GetPaged([FromQuery] ProdutoFiltroDto filtro, CancellationToken cancellationToken)
    {
        var result = await _produtoService.GetPagedAsync(filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("ativos-para-venda")]
    public async Task<ActionResult<IReadOnlyList<ProdutoDto>>> GetAllAtivosParaVenda(CancellationToken cancellationToken)
    {
        var result = await _produtoService.GetAllAtivosParaVendaAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _produtoService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Create([FromBody] ProdutoCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _produtoService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> Update(int id, [FromBody] ProdutoUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _produtoService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancellationToken)
    {
        await _produtoService.InativarAsync(id, cancellationToken);
        return Ok(new { message = "Produto inativado com sucesso." });
    }

    [HttpPatch("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id, CancellationToken cancellationToken)
    {
        await _produtoService.ReativarAsync(id, cancellationToken);
        return Ok(new { message = "Produto reativado com sucesso." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _produtoService.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Produto excluído com sucesso." });
    }
}
