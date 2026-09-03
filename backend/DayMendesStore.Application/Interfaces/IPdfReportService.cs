using DayMendesStore.Application.DTOs;

namespace DayMendesStore.Application.Interfaces;

public interface IPdfReportService
{
    Task<byte[]> GerarRelatorioCompletoPdfAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
}
