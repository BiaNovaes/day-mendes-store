using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IUnitOfWork _unitOfWork;

    public EstoqueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EstoqueProdutoDto>> GetSaldoEstoqueAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _unitOfWork.Produtos.Query()
            .Where(p => p.Status == Status.Ativo)
            .Include(p => p.Variacoes.Where(v => v.Status == Status.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return produtos.Select(p => new EstoqueProdutoDto
        {
            ProdutoId = p.Id,
            Nome = p.Nome,
            Marca = p.Marca,
            EstoqueMinimo = p.EstoqueMinimo,
            EstoqueTotal = p.Variacoes.Sum(v => v.QuantidadeEstoque),
            Variacoes = p.Variacoes.Select(MapToDto).ToList()
        }).ToList();
    }

    public async Task<EstoqueProdutoDto> GetSaldoPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default)
    {
        var produto = await _unitOfWork.Produtos.Query()
            .Where(p => p.Id == produtoId && p.Status == Status.Ativo)
            .Include(p => p.Variacoes.Where(v => v.Status == Status.Ativo))
            .FirstOrDefaultAsync(cancellationToken);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID {produtoId} não encontrado.");
        }

        return new EstoqueProdutoDto
        {
            ProdutoId = produto.Id,
            Nome = produto.Nome,
            Marca = produto.Marca,
            EstoqueMinimo = produto.EstoqueMinimo,
            EstoqueTotal = produto.Variacoes.Where(v => v.Status == Status.Ativo).Sum(v => v.QuantidadeEstoque),
            Variacoes = produto.Variacoes.Select(MapToDto).ToList()
        };
    }

    public async Task<EstoqueVariacaoDto> AtualizarQuantidadeAsync(int variacaoId, int novaQuantidade, CancellationToken cancellationToken = default)
    {
        if (novaQuantidade < 0)
        {
            throw new BusinessException("A quantidade em estoque não pode ser negativa.");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var variacao = await _unitOfWork.VariacoesProduto.GetByIdForUpdateAsync(variacaoId, cancellationToken);
            if (variacao == null)
            {
                throw new NotFoundException($"Variação com ID {variacaoId} não encontrada.");
            }

            variacao.QuantidadeEstoque = novaQuantidade;
            _unitOfWork.VariacoesProduto.Update(variacao);

            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapToDto(variacao);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<EstoqueVariacaoDto> RegistrarEntradaAsync(int variacaoId, int quantidade, CancellationToken cancellationToken = default)
    {
        if (quantidade <= 0)
        {
            throw new BusinessException("A quantidade de entrada deve ser maior que zero.");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var variacao = await _unitOfWork.VariacoesProduto.GetByIdForUpdateAsync(variacaoId, cancellationToken);
            if (variacao == null)
            {
                throw new NotFoundException($"Variação com ID {variacaoId} não encontrada.");
            }

            variacao.QuantidadeEstoque += quantidade;
            _unitOfWork.VariacoesProduto.Update(variacao);

            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapToDto(variacao);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<EstoqueVariacaoDto> RegistrarSaidaAsync(int variacaoId, int quantidade, CancellationToken cancellationToken = default)
    {
        if (quantidade <= 0)
        {
            throw new BusinessException("A quantidade de saída deve ser maior que zero.");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var variacao = await _unitOfWork.VariacoesProduto.GetByIdForUpdateAsync(variacaoId, cancellationToken);
            if (variacao == null)
            {
                throw new NotFoundException($"Variação com ID {variacaoId} não encontrada.");
            }

            if (variacao.QuantidadeEstoque < quantidade)
            {
                throw new BusinessException($"Estoque insuficiente para dar saída na variação '{variacao.Tamanho}/{variacao.Cor}'. Atual: {variacao.QuantidadeEstoque}, Solicitado: {quantidade}.");
            }

            variacao.QuantidadeEstoque -= quantidade;
            _unitOfWork.VariacoesProduto.Update(variacao);

            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapToDto(variacao);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static EstoqueVariacaoDto MapToDto(VariacaoProduto variacao)
    {
        return new EstoqueVariacaoDto
        {
            VariacaoId = variacao.Id,
            ProdutoId = variacao.ProdutoId,
            Tamanho = variacao.Tamanho,
            Cor = variacao.Cor,
            QuantidadeEstoque = variacao.QuantidadeEstoque
        };
    }
}
