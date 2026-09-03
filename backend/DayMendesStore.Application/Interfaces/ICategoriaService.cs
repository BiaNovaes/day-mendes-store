using DayMendesStore.Application.DTOs;
using DayMendesStore.Domain.Enums;

namespace DayMendesStore.Application.Interfaces;

public interface ICategoriaService
{
    Task<PagedResultDto<CategoriaDto>> GetPagedAsync(PaginationParamsDto pagination, Status? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaDto>> GetAllAtivasAsync(CancellationToken cancellationToken = default);
    Task<CategoriaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoriaDto> CreateAsync(CategoriaCreateDto dto, CancellationToken cancellationToken = default);
    Task<CategoriaDto> UpdateAsync(int id, CategoriaUpdateDto dto, CancellationToken cancellationToken = default);
    Task InativarAsync(int id, CancellationToken cancellationToken = default);
    Task ReativarAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IProdutoService
{
    Task<PagedResultDto<ProdutoDto>> GetPagedAsync(ProdutoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProdutoDto>> GetAllAtivosParaVendaAsync(CancellationToken cancellationToken = default);
    Task<ProdutoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProdutoDto> CreateAsync(ProdutoCreateDto dto, CancellationToken cancellationToken = default);
    Task<ProdutoDto> UpdateAsync(int id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default);
    Task InativarAsync(int id, CancellationToken cancellationToken = default);
    Task ReativarAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
