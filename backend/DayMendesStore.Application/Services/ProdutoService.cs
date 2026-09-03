using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<ProdutoDto>> GetPagedAsync(ProdutoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.TermoBusca))
        {
            var termo = filtro.TermoBusca.Trim().ToLower();
            query = query.Where(p => p.Nome.ToLower().Contains(termo) 
                                  || (p.Descricao != null && p.Descricao.ToLower().Contains(termo))
                                  || (p.Marca != null && p.Marca.ToLower().Contains(termo))
                                  || p.Variacoes.Any(v => v.Status != Status.Deletado 
                                                       && (v.Cor.ToLower().Contains(termo) || v.Tamanho.ToLower().Contains(termo))));
        }

        if (filtro.CategoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tamanho))
        {
            var tam = filtro.Tamanho.Trim().ToLower();
            query = query.Where(p => p.Variacoes.Any(v => v.Status != Status.Deletado && v.Tamanho.ToLower() == tam));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Cor))
        {
            var cor = filtro.Cor.Trim().ToLower();
            query = query.Where(p => p.Variacoes.Any(v => v.Status != Status.Deletado && v.Cor.ToLower() == cor));
        }

        if (filtro.Status.HasValue)
        {
            query = query.Where(p => p.Status == filtro.Status.Value);
        }

        if (filtro.ApenasEmEstoque == true)
        {
            query = query.Where(p => p.Variacoes.Any(v => v.Status == Status.Ativo && v.QuantidadeEstoque > 0));
        }

        if (filtro.ApenasEstoqueBaixo == true)
        {
            query = query.Where(p => p.Variacoes.Where(v => v.Status == Status.Ativo).Sum(v => v.QuantidadeEstoque) <= p.EstoqueMinimo);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Nome)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResultDto<ProdutoDto>(dtos, totalCount, filtro.Page, filtro.PageSize);
    }

    public async Task<IReadOnlyList<ProdutoDto>> GetAllAtivosParaVendaAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .Where(p => p.Status == Status.Ativo 
                     && p.Categoria != null && p.Categoria.Status == Status.Ativo 
                     && p.Variacoes.Any(v => v.Status == Status.Ativo && v.QuantidadeEstoque > 0))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return produtos.Select(MapToDto).ToList();
    }

    public async Task<ProdutoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _unitOfWork.Produtos.Query()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID {id} não encontrado.");
        }

        return MapToDto(produto);
    }

    public async Task<ProdutoDto> CreateAsync(ProdutoCreateDto dto, CancellationToken cancellationToken = default)
    {
        ValidateProdutoValues(dto.Nome, dto.ValorCompra, dto.ValorVenda, dto.EstoqueMinimo);

        var nomeTrim = dto.Nome.Trim();

        if (dto.Variacoes != null && dto.Variacoes.Any())
        {
            var dupInList = dto.Variacoes
                .GroupBy(v => new { Tamanho = (v.Tamanho ?? string.Empty).Trim().ToLower(), Cor = (v.Cor ?? string.Empty).Trim().ToLower() })
                .FirstOrDefault(g => g.Count() > 1);

            if (dupInList != null)
            {
                throw new BusinessException($"Não é permitido cadastrar variações duplicadas com o mesmo tamanho e cor ('{dupInList.Key.Tamanho}/{dupInList.Key.Cor}') no mesmo produto.");
            }

            foreach (var v in dto.Variacoes)
            {
                if (v.QuantidadeEstoque < 0)
                {
                    throw new BusinessException("A quantidade em estoque de uma variação não pode ser negativa.");
                }
            }
        }

        var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.CategoriaId, cancellationToken);
        if (categoria == null || categoria.Status != Status.Ativo)
        {
            throw new BusinessException("A categoria informada não existe ou está inativa.");
        }

        var produto = new Produto
        {
            CategoriaId = dto.CategoriaId,
            Foto = dto.Foto,
            Nome = nomeTrim,
            Descricao = dto.Descricao?.Trim(),
            Marca = dto.Marca?.Trim(),
            ValorCompra = dto.ValorCompra,
            ValorVenda = dto.ValorVenda,
            EstoqueMinimo = dto.EstoqueMinimo,
            Status = Status.Ativo,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Variacoes != null && dto.Variacoes.Any())
        {
            foreach (var vDto in dto.Variacoes)
            {
                var variacao = new VariacaoProduto
                {
                    Produto = produto,
                    Tamanho = vDto.Tamanho?.Trim() ?? string.Empty,
                    Cor = vDto.Cor?.Trim() ?? string.Empty,
                    QuantidadeEstoque = vDto.QuantidadeEstoque,
                    Status = Status.Ativo,
                    CreatedAt = DateTime.UtcNow
                };

                produto.Variacoes.Add(variacao);

                if (vDto.QuantidadeEstoque > 0)
                {
                    var mov = new MovimentacaoEstoque
                    {
                        Produto = produto,
                        VariacaoProduto = variacao,
                        Tipo = TipoMovimentacaoEstoque.Entrada,
                        Quantidade = vDto.QuantidadeEstoque,
                        Motivo = null,
                        DataMovimentacao = DateTime.UtcNow,
                        Status = Status.Ativo,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.MovimentacoesEstoque.AddAsync(mov, cancellationToken);
                }
            }
        }

        await _unitOfWork.Produtos.AddAsync(produto, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return await GetByIdAsync(produto.Id, cancellationToken);
    }

    public async Task<ProdutoDto> UpdateAsync(int id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        ValidateProdutoValues(dto.Nome, dto.ValorCompra, dto.ValorVenda, dto.EstoqueMinimo);

        var produto = await _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID {id} não encontrado.");
        }

        if (produto.CategoriaId != dto.CategoriaId)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.CategoriaId, cancellationToken);
            if (categoria == null || categoria.Status != Status.Ativo)
            {
                throw new BusinessException("A categoria informada não existe ou está inativa.");
            }
            produto.CategoriaId = dto.CategoriaId;
        }

        produto.Nome = dto.Nome.Trim();
        produto.Descricao = dto.Descricao?.Trim();
        produto.Marca = dto.Marca?.Trim();
        produto.Foto = dto.Foto ?? produto.Foto;
        produto.ValorCompra = dto.ValorCompra;
        produto.ValorVenda = dto.ValorVenda;
        produto.EstoqueMinimo = dto.EstoqueMinimo;

        if (dto.Variacoes != null)
        {
            var activeIncoming = dto.Variacoes.Where(v => v.Status != Status.Deletado).ToList();
            var dupInList = activeIncoming
                .GroupBy(v => new { Tamanho = (v.Tamanho ?? string.Empty).Trim().ToLower(), Cor = (v.Cor ?? string.Empty).Trim().ToLower() })
                .FirstOrDefault(g => g.Count() > 1);

            if (dupInList != null)
            {
                throw new BusinessException($"Não é permitido cadastrar variações duplicadas com o mesmo tamanho e cor ('{dupInList.Key.Tamanho}/{dupInList.Key.Cor}') no mesmo produto.");
            }

            foreach (var vDto in dto.Variacoes)
            {
                if (vDto.QuantidadeEstoque < 0)
                {
                    throw new BusinessException("A quantidade em estoque de uma variação não pode ser negativa.");
                }
            }

            // Fetch ALL variations in DB for this product (including soft-deleted and inactive) to avoid unique constraint collisions
            var allVarsInDb = await _unitOfWork.VariacoesProduto.QueryIgnoreFilters()
                .Where(v => v.ProdutoId == id)
                .ToListAsync(cancellationToken);

            var touchedVarIds = new HashSet<int>();

            foreach (var vDto in dto.Variacoes)
            {
                var normTamanho = vDto.Tamanho?.Trim() ?? string.Empty;
                var normCor = vDto.Cor?.Trim() ?? string.Empty;
                var normTamanhoLower = normTamanho.ToLower();
                var normCorLower = normCor.ToLower();

                if (vDto.Id.HasValue && vDto.Id.Value > 0)
                {
                    var existingVar = allVarsInDb.FirstOrDefault(v => v.Id == vDto.Id.Value);
                    if (existingVar != null)
                    {
                        var conflict = allVarsInDb.FirstOrDefault(v => v.Id != existingVar.Id 
                            && v.Tamanho.Trim().ToLower() == normTamanhoLower 
                            && v.Cor.Trim().ToLower() == normCorLower);

                        if (conflict != null)
                        {
                            throw new BusinessException($"Não é permitido alterar a variação para '{normTamanho}/{normCor}' pois essa combinação já existe para este produto.");
                        }

                        var diferencaEstoque = vDto.QuantidadeEstoque - existingVar.QuantidadeEstoque;
                        if (diferencaEstoque != 0)
                        {
                            var mov = new MovimentacaoEstoque
                            {
                                ProdutoId = produto.Id,
                                VariacaoProdutoId = existingVar.Id,
                                Tipo = TipoMovimentacaoEstoque.Ajuste,
                                Quantidade = Math.Abs(diferencaEstoque),
                                Motivo = null,
                                DataMovimentacao = DateTime.UtcNow,
                                Status = Status.Ativo,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.MovimentacoesEstoque.AddAsync(mov, cancellationToken);
                        }

                        existingVar.Tamanho = normTamanho;
                        existingVar.Cor = normCor;
                        existingVar.QuantidadeEstoque = vDto.QuantidadeEstoque;
                        if (vDto.Status.HasValue)
                        {
                            existingVar.Status = vDto.Status.Value;
                        }
                        else if (existingVar.Status == Status.Deletado)
                        {
                            existingVar.Status = Status.Ativo;
                        }
                        existingVar.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.VariacoesProduto.Update(existingVar);
                        touchedVarIds.Add(existingVar.Id);
                    }
                }
                else
                {
                    // No ID provided. Check if a variation with this exact (Tamanho, Cor) already exists in DB (even if Deletado or Inativo)
                    var existingWithSameAttributes = allVarsInDb.FirstOrDefault(v => 
                        v.Tamanho.Trim().ToLower() == normTamanhoLower && 
                        v.Cor.Trim().ToLower() == normCorLower);

                    if (existingWithSameAttributes != null)
                    {
                        var diferencaEstoque = vDto.QuantidadeEstoque - existingWithSameAttributes.QuantidadeEstoque;
                        var eraInativoOuDeletado = existingWithSameAttributes.Status == Status.Deletado || existingWithSameAttributes.Status == Status.Inativo;

                        existingWithSameAttributes.Tamanho = normTamanho;
                        existingWithSameAttributes.Cor = normCor;
                        existingWithSameAttributes.QuantidadeEstoque = vDto.QuantidadeEstoque;
                        existingWithSameAttributes.Status = vDto.Status ?? Status.Ativo;
                        existingWithSameAttributes.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.VariacoesProduto.Update(existingWithSameAttributes);
                        touchedVarIds.Add(existingWithSameAttributes.Id);

                        if (!produto.Variacoes.Any(v => v.Id == existingWithSameAttributes.Id))
                        {
                            produto.Variacoes.Add(existingWithSameAttributes);
                        }

                        if (eraInativoOuDeletado && vDto.QuantidadeEstoque > 0)
                        {
                            var mov = new MovimentacaoEstoque
                            {
                                ProdutoId = produto.Id,
                                VariacaoProdutoId = existingWithSameAttributes.Id,
                                Tipo = TipoMovimentacaoEstoque.Entrada,
                                Quantidade = vDto.QuantidadeEstoque,
                                Motivo = null,
                                DataMovimentacao = DateTime.UtcNow,
                                Status = Status.Ativo,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.MovimentacoesEstoque.AddAsync(mov, cancellationToken);
                        }
                        else if (diferencaEstoque != 0)
                        {
                            var mov = new MovimentacaoEstoque
                            {
                                ProdutoId = produto.Id,
                                VariacaoProdutoId = existingWithSameAttributes.Id,
                                Tipo = TipoMovimentacaoEstoque.Ajuste,
                                Quantidade = Math.Abs(diferencaEstoque),
                                Motivo = null,
                                DataMovimentacao = DateTime.UtcNow,
                                Status = Status.Ativo,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.MovimentacoesEstoque.AddAsync(mov, cancellationToken);
                        }
                    }
                    else
                    {
                        var newVar = new VariacaoProduto
                        {
                            Produto = produto,
                            ProdutoId = produto.Id,
                            Tamanho = normTamanho,
                            Cor = normCor,
                            QuantidadeEstoque = vDto.QuantidadeEstoque,
                            Status = vDto.Status ?? Status.Ativo,
                            CreatedAt = DateTime.UtcNow
                        };

                        produto.Variacoes.Add(newVar);
                        await _unitOfWork.VariacoesProduto.AddAsync(newVar, cancellationToken);

                        if (vDto.QuantidadeEstoque > 0)
                        {
                            var mov = new MovimentacaoEstoque
                            {
                                Produto = produto,
                                VariacaoProduto = newVar,
                                Tipo = TipoMovimentacaoEstoque.Entrada,
                                Quantidade = vDto.QuantidadeEstoque,
                                Motivo = null,
                                DataMovimentacao = DateTime.UtcNow,
                                Status = Status.Ativo,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.MovimentacoesEstoque.AddAsync(mov, cancellationToken);
                        }
                    }
                }
            }

            // Remove/inactivate active variations that were omitted from the update payload
            var activeDbVars = allVarsInDb.Where(v => v.Status != Status.Deletado).ToList();
            foreach (var activeVar in activeDbVars)
            {
                if (!touchedVarIds.Contains(activeVar.Id))
                {
                    var temVendas = await _unitOfWork.ItensVenda.QueryIgnoreFilters()
                        .AnyAsync(iv => iv.VariacaoProdutoId == activeVar.Id, cancellationToken);

                    if (temVendas)
                    {
                        activeVar.Status = Status.Inativo;
                    }
                    else
                    {
                        activeVar.Status = Status.Deletado;
                    }

                    activeVar.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.VariacoesProduto.Update(activeVar);
                }
            }
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task InativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id, cancellationToken);
        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID {id} não encontrado.");
        }

        if (produto.Status == Status.Inativo)
        {
            return;
        }

        produto.Status = Status.Inativo;
        _unitOfWork.Produtos.Update(produto);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task ReativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID {id} não encontrado.");
        }

        if (produto.Categoria == null || produto.Categoria.Status != Status.Ativo)
        {
            throw new BusinessException("Não é possível reativar o produto pois a categoria vinculada está inativa.");
        }

        if (produto.Status == Status.Ativo)
        {
            return;
        }

        produto.Status = Status.Ativo;
        _unitOfWork.Produtos.Update(produto);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _unitOfWork.Produtos.Query()
            .Include(p => p.Variacoes)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID {id} não encontrado.");
        }

        var temVendas = await _unitOfWork.ItensVenda.Query()
            .AnyAsync(iv => iv.ProdutoId == id, cancellationToken);

        if (temVendas)
        {
            throw new BusinessException("Não é possível excluir um produto que possui histórico de vendas. Inative o produto para removê-lo de novas vendas.");
        }

        produto.Status = Status.Deletado;
        foreach (var v in produto.Variacoes)
        {
            v.Status = Status.Deletado;
        }

        _unitOfWork.Produtos.Update(produto);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    private static void ValidateProdutoValues(string nome, decimal valorCompra, decimal valorVenda, int estoqueMinimo)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new BusinessException("O nome do produto é obrigatório.");
        }

        if (valorCompra < 0)
        {
            throw new BusinessException("O valor de compra não pode ser negativo.");
        }

        if (valorVenda < 0)
        {
            throw new BusinessException("O valor de venda não pode ser negativo.");
        }

        if (estoqueMinimo < 0)
        {
            throw new BusinessException("O estoque mínimo não pode ser negativo.");
        }
    }

    private static ProdutoDto MapToDto(Produto p) => new()
    {
        Id = p.Id,
        CategoriaId = p.CategoriaId,
        CategoriaNome = p.Categoria?.Nome ?? string.Empty,
        Foto = p.Foto,
        Nome = p.Nome,
        Descricao = p.Descricao,
        Marca = p.Marca,
        ValorCompra = p.ValorCompra,
        ValorVenda = p.ValorVenda,
        EstoqueMinimo = p.EstoqueMinimo,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Variacoes = p.Variacoes
            .Where(v => v.Status != Status.Deletado)
            .Select(v => new VariacaoProdutoDto
            {
                Id = v.Id,
                ProdutoId = v.ProdutoId,
                Tamanho = v.Tamanho,
                Cor = v.Cor,
                QuantidadeEstoque = v.QuantidadeEstoque,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .OrderBy(v => v.Tamanho)
            .ThenBy(v => v.Cor)
            .ToList()
    };
}
