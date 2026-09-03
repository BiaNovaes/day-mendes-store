using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class AuthController : ApiBaseController
{
    private readonly IAuthService _authService;
    private readonly ILojaService _lojaService;

    public AuthController(IAuthService authService, ILojaService lojaService)
    {
        _authService = authService;
        _lojaService = lojaService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("setup-loja")]
    [AllowAnonymous]
    public async Task<ActionResult<LojaDto>> SetupLoja([FromBody] SetupLojaDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.SetupLojaAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPerfil), response);
    }

    [HttpGet("perfil")]
    public async Task<ActionResult<LojaDto>> GetPerfil(CancellationToken cancellationToken)
    {
        var loja = await _lojaService.GetLojaAsync(cancellationToken);
        return Ok(loja);
    }
}
