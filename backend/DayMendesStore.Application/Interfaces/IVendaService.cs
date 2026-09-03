using DayMendesStore.Application.DTOs;

namespace DayMendesStore.Application.Interfaces;

public interface IVendaService
{
    Task<PagedResultDto<VendaDto>> GetPagedAsync(VendaFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<VendaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<VendaDto> CreateAsync(VendaCreateDto dto, CancellationToken cancellationToken = default);
    Task<VendaDto> UpdateAsync(int id, VendaUpdateDto dto, CancellationToken cancellationToken = default);
    Task<VendaDto> FinalizarAsync(int id, CancellationToken cancellationToken = default);
    Task<VendaDto> CancelarAsync(int id, string? motivo = null, CancellationToken cancellationToken = default);
    Task DeleteRascunhoAsync(int id, CancellationToken cancellationToken = default);
}
