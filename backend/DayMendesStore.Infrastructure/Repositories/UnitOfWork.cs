using System.Collections.Concurrent;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DayMendesStore.Infrastructure.Repositories;

public class InMemoryTransaction : IDbContextTransaction
{
    private readonly Action? _onComplete;
    private bool _disposed;

    public InMemoryTransaction(Action? onComplete = null)
    {
        _onComplete = onComplete;
        TransactionId = Guid.NewGuid();
    }

    public Guid TransactionId { get; }

    public void Commit() => _onComplete?.Invoke();
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _onComplete?.Invoke();
        return Task.CompletedTask;
    }

    public void Rollback() => _onComplete?.Invoke();
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _onComplete?.Invoke();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _onComplete?.Invoke();
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}

public class UnitOfWork : IUnitOfWork
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _inMemoryDbSemaphores = new();

    private readonly AppDbContext _context;

    private IRepository<Loja>? _lojas;
    private IRepository<Categoria>? _categorias;
    private IRepository<Produto>? _produtos;
    private IRepository<VariacaoProduto>? _variacoesProduto;
    private IRepository<Cliente>? _clientes;
    private IRepository<Venda>? _vendas;
    private IRepository<ItemVenda>? _itensVenda;
    private IRepository<MovimentacaoEstoque>? _movimentacoesEstoque;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Loja> Lojas => _lojas ??= new Repository<Loja>(_context);
    public IRepository<Categoria> Categorias => _categorias ??= new Repository<Categoria>(_context);
    public IRepository<Produto> Produtos => _produtos ??= new Repository<Produto>(_context);
    public IRepository<VariacaoProduto> VariacoesProduto => _variacoesProduto ??= new Repository<VariacaoProduto>(_context);
    public IRepository<Cliente> Clientes => _clientes ??= new Repository<Cliente>(_context);
    public IRepository<Venda> Vendas => _vendas ??= new Repository<Venda>(_context);
    public IRepository<ItemVenda> ItensVenda => _itensVenda ??= new Repository<ItemVenda>(_context);
    public IRepository<MovimentacaoEstoque> MovimentacoesEstoque => _movimentacoesEstoque ??= new Repository<MovimentacaoEstoque>(_context);

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.IsRelational())
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        var dbName = _context.Database.ProviderName ?? "InMemory";
        var semaphore = _inMemoryDbSemaphores.GetOrAdd(dbName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        var released = 0;
        return new InMemoryTransaction(() =>
        {
            if (Interlocked.Exchange(ref released, 1) == 0)
            {
                semaphore.Release();
            }
        });
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
