using DayMendesStore.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace DayMendesStore.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Loja> Lojas { get; }
    IRepository<Categoria> Categorias { get; }
    IRepository<Produto> Produtos { get; }
    IRepository<VariacaoProduto> VariacoesProduto { get; }
    IRepository<Cliente> Clientes { get; }
    IRepository<Venda> Vendas { get; }
    IRepository<ItemVenda> ItensVenda { get; }
    IRepository<MovimentacaoEstoque> MovimentacoesEstoque { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
