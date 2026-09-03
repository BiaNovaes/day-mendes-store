using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class LojaController : ApiBaseController
{
    private readonly ILojaService _lojaService;

    public LojaController(ILojaService lojaService)
    {
        _lojaService = lojaService;
    }

    [HttpGet]
    public async Task<ActionResult<LojaDto>> Get(CancellationToken cancellationToken)
    {
        var loja = await _lojaService.GetLojaAsync(cancellationToken);
        return Ok(loja);
    }

    [HttpPut]
    public async Task<ActionResult<LojaDto>> Update([FromBody] LojaUpdateRequestDto request, CancellationToken cancellationToken)
    {
        var loja = await _lojaService.UpdateLojaAsync(request, cancellationToken);
        return Ok(loja);
    }

    [HttpPut("senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto request, CancellationToken cancellationToken)
    {
        await _lojaService.AlterarSenhaAsync(request, cancellationToken);
        return Ok(new { message = "Senha alterada com sucesso." });
    }
}
