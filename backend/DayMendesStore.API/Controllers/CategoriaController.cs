using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class CategoriaController : ApiBaseController
{
    private readonly ICategoriaService _categoriaService;

    public CategoriaController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CategoriaDto>>> GetPaged([FromQuery] PaginationParamsDto pagination, [FromQuery] Status? status, CancellationToken cancellationToken)
    {
        var result = await _categoriaService.GetPagedAsync(pagination, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("ativas")]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> GetAllAtivas(CancellationToken cancellationToken)
    {
        var result = await _categoriaService.GetAllAtivasAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _categoriaService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CategoriaCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _categoriaService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> Update(int id, [FromBody] CategoriaUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _categoriaService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancellationToken)
    {
        await _categoriaService.InativarAsync(id, cancellationToken);
        return Ok(new { message = "Categoria inativada com sucesso." });
    }

    [HttpPatch("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id, CancellationToken cancellationToken)
    {
        await _categoriaService.ReativarAsync(id, cancellationToken);
        return Ok(new { message = "Categoria reativada com sucesso." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _categoriaService.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Categoria excluída com sucesso." });
    }
}
