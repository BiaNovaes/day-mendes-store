using DayMendesStore.Application.DTOs;

namespace DayMendesStore.Application.Interfaces;

public interface IEstoqueService
{
    Task<IReadOnlyList<EstoqueProdutoDto>> GetSaldoEstoqueAsync(CancellationToken cancellationToken = default);
    Task<EstoqueProdutoDto> GetSaldoPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default);
    Task<EstoqueVariacaoDto> AtualizarQuantidadeAsync(int variacaoId, int novaQuantidade, CancellationToken cancellationToken = default);
    Task<EstoqueVariacaoDto> RegistrarEntradaAsync(int variacaoId, int quantidade, CancellationToken cancellationToken = default);
    Task<EstoqueVariacaoDto> RegistrarSaidaAsync(int variacaoId, int quantidade, CancellationToken cancellationToken = default);
}
