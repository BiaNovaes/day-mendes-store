using DayMendesStore.Application.DTOs;

namespace DayMendesStore.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<LojaDto> SetupLojaAsync(SetupLojaDto request, CancellationToken cancellationToken = default);
}

public interface ILojaService
{
    Task<LojaDto> GetLojaAsync(CancellationToken cancellationToken = default);
    Task<LojaDto> UpdateLojaAsync(LojaUpdateRequestDto request, CancellationToken cancellationToken = default);
    Task AlterarSenhaAsync(AlterarSenhaDto request, CancellationToken cancellationToken = default);
    Task<string> AtualizarFotoAsync(string fotoUrl, CancellationToken cancellationToken = default);
}
