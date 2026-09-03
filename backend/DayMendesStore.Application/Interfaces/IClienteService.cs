using DayMendesStore.Application.DTOs;
using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.Interfaces;

public interface IClienteService
{
    Task<PagedResultDto<ClienteDto>> GetPagedAsync(ClienteFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClienteDto>> GetAllAtivosAsync(CancellationToken cancellationToken = default);
    Task<ClienteDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClienteDto> CreateAsync(ClienteCreateDto dto, CancellationToken cancellationToken = default);
    Task<ClienteDto> UpdateAsync(int id, ClienteUpdateDto dto, CancellationToken cancellationToken = default);
    Task InativarAsync(int id, CancellationToken cancellationToken = default);
    Task ReativarAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
