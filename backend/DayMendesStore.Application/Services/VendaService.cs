using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class VendaService : IVendaService
{
    private readonly IUnitOfWork _unitOfWork;

    public VendaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<VendaDto>> GetPagedAsync(VendaFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Vendas.Query()
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Include(v => v.Itens)
                .ThenInclude(i => i.VariacaoProduto)
            .AsQueryable();

        if (filtro.DataInicio.HasValue)
        {
            var dataInicioUtc = DateTime.SpecifyKind(filtro.DataInicio.Value, DateTimeKind.Utc);
            query = query.Where(v => v.DataVenda >= dataInicioUtc);
        }

        if (filtro.DataFim.HasValue)
        {
            var dataFimUtc = DateTime.SpecifyKind(filtro.DataFim.Value, DateTimeKind.Utc);
            query = query.Where(v => v.DataVenda <= dataFimUtc);
        }

        if (filtro.ClienteId.HasValue)
        {
            query = query.Where(v => v.ClienteId == filtro.ClienteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.FormaPagamento))
        {
            var fp = filtro.FormaPagamento.Trim().ToLower();
            query = query.Where(v => v.FormaPagamento.ToLower() == fp);
        }

        if (filtro.StatusVenda.HasValue)
        {
            query = query.Where(v => v.StatusVenda == filtro.StatusVenda.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.DataVenda)
            .ThenByDescending(v => v.Id)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResultDto<VendaDto>(dtos, totalCount, filtro.Page, filtro.PageSize);
    }

    public async Task<VendaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var venda = await _unitOfWork.Vendas.Query()
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Include(v => v.Itens)
                .ThenInclude(i => i.VariacaoProduto)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (venda == null)
        {
            throw new NotFoundException($"Venda com ID {id} não encontrada.");
        }

        return MapToDto(venda);
    }

    public async Task<VendaDto> CreateAsync(VendaCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FormaPagamento))
        {
            throw new BusinessException("A forma de pagamento é obrigatória.");
        }

        if (dto.Itens == null || !dto.Itens.Any())
        {
            throw new BusinessException("A venda deve conter pelo menos um item.");
        }

        if (dto.ClienteId.HasValue)
        {
            var clienteExiste = await _unitOfWork.Clientes.Query().AnyAsync(c => c.Id == dto.ClienteId.Value, cancellationToken);
            if (!clienteExiste)
            {
                throw new BusinessException("O cliente informado não existe ou está inativo.");
            }
        }

        foreach (var item in dto.Itens)
        {
            if (item.Quantidade <= 0)
            {
                throw new BusinessException("A quantidade de cada item deve ser maior que zero.");
            }
        }

        var dataVenda = dto.DataVenda.HasValue
            ? DateTime.SpecifyKind(dto.DataVenda.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        var venda = new Venda
        {
            ClienteId = dto.ClienteId,
            DataVenda = dataVenda,
            FormaPagamento = dto.FormaPagamento.Trim(),
            StatusVenda = dto.FinalizarImediatamente ? StatusVenda.Finalizada : StatusVenda.Rascunho,
            Status = Status.Ativo,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.FinalizarImediatamente)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var produtoIds = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
                var variacaoIds = dto.Itens.Select(i => i.VariacaoProdutoId).Distinct().OrderBy(id => id).ToList();

                var variacoesList = await _unitOfWork.VariacoesProduto.GetByIdsForUpdateAsync(variacaoIds, cancellationToken);
                var variacoes = variacoesList.ToDictionary(v => v.Id);

                var produtos = await _unitOfWork.Produtos.Query()
                    .Include(p => p.Categoria)
                    .Where(p => produtoIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, cancellationToken);

                var totalQuantityPerVariation = dto.Itens
                    .GroupBy(i => i.VariacaoProdutoId)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));

                decimal valorTotal = 0;

                foreach (var itemDto in dto.Itens)
                {
                    if (!produtos.TryGetValue(itemDto.ProdutoId, out var produto))
                    {
                        throw new BusinessException($"Produto com ID {itemDto.ProdutoId} não encontrado.");
                    }

                    if (produto.Status != Status.Ativo)
                    {
                        throw new BusinessException($"O produto '{produto.Nome}' está inativo e não pode ser vendido.");
                    }

                    if (produto.Categoria == null || produto.Categoria.Status != Status.Ativo)
                    {
                        throw new BusinessException($"A categoria do produto '{produto.Nome}' está inativa.");
                    }

                    if (!variacoes.TryGetValue(itemDto.VariacaoProdutoId, out var variacao))
                    {
                        throw new BusinessException($"Variação com ID {itemDto.VariacaoProdutoId} não encontrada.");
                    }

                    if (variacao.ProdutoId != produto.Id)
                    {
                        throw new BusinessException($"A variação ID {itemDto.VariacaoProdutoId} não pertence ao produto '{produto.Nome}'.");
                    }

                    if (variacao.Status != Status.Ativo)
                    {
                        throw new BusinessException($"A variação '{variacao.Tamanho}/{variacao.Cor}' do produto '{produto.Nome}' está inativa e não pode ser vendida.");
                    }

                    var subtotal = produto.ValorVenda * itemDto.Quantidade;
                    valorTotal += subtotal;

                    venda.Itens.Add(new ItemVenda
                    {
                        ProdutoId = produto.Id,
                        VariacaoProdutoId = variacao.Id,
                        Quantidade = itemDto.Quantidade,
                        ValorUnitario = produto.ValorVenda,
                        Subtotal = subtotal,
                        Status = Status.Ativo,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var (varId, requestedQty) in totalQuantityPerVariation)
                {
                    var variacao = variacoes[varId];
                    var produto = produtos[variacao.ProdutoId];

                    if (variacao.QuantidadeEstoque < requestedQty)
                    {
                        throw new BusinessException($"Estoque insuficiente para a variação '{variacao.Tamanho}/{variacao.Cor}' do produto '{produto.Nome}'. Disponível: {variacao.QuantidadeEstoque}, Solicitado: {requestedQty}.");
                    }

                    variacao.QuantidadeEstoque -= requestedQty;
                    _unitOfWork.VariacoesProduto.Update(variacao);
                }

                venda.ValorTotal = valorTotal;

                await _unitOfWork.Vendas.AddAsync(venda, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                foreach (var item in venda.Itens)
                {
                    var mov = new MovimentacaoEstoque
                    {
                        ProdutoId = item.ProdutoId,
                        VariacaoProdutoId = item.VariacaoProdutoId,
                        Tipo = TipoMovimentacaoEstoque.Saida,
                        Quantidade = item.Quantidade,
                        Motivo = null,
                        DataMovimentacao = venda.DataVenda,
                        Status = Status.Ativo,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.MovimentacoesEstoque.AddAsync(mov, cancellationToken);
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetByIdAsync(venda.Id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        else
        {
            var produtoIds = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
            var variacaoIds = dto.Itens.Select(i => i.VariacaoProdutoId).Distinct().ToList();

            var produtos = await _unitOfWork.Produtos.Query()
                .Where(p => produtoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var variacoes = await _unitOfWork.VariacoesProduto.Query()
                .Where(v => variacaoIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, cancellationToken);

            decimal valorTotal = 0;

            foreach (var itemDto in dto.Itens)
            {
                if (!produtos.TryGetValue(itemDto.ProdutoId, out var produto))
                {
                    throw new BusinessException($"Produto com ID {itemDto.ProdutoId} não encontrado.");
                }

                if (!variacoes.TryGetValue(itemDto.VariacaoProdutoId, out var variacao))
                {
                    throw new BusinessException($"Variação com ID {itemDto.VariacaoProdutoId} não encontrada.");
                }

                if (variacao.ProdutoId != produto.Id)
                {
                    throw new BusinessException($"A variação ID {itemDto.VariacaoProdutoId} não pertence ao produto '{produto.Nome}'.");
                }

                var subtotal = produto.ValorVenda * itemDto.Quantidade;
                valorTotal += subtotal;

                venda.Itens.Add(new ItemVenda
                {
                    ProdutoId = produto.Id,
                    VariacaoProdutoId = variacao.Id,
                    Quantidade = itemDto.Quantidade,
                    ValorUnitario = produto.ValorVenda,
                    Subtotal = subtotal,
                    Status = Status.Ativo,
                    CreatedAt = DateTime.UtcNow
                });
            }

            venda.ValorTotal = valorTotal;

            await _unitOfWork.Vendas.AddAsync(venda, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return await GetByIdAsync(venda.Id, cancellationToken);
        }
    }

    public async Task<VendaDto> UpdateAsync(int id, VendaUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var venda = await _unitOfWork.Vendas.Query()
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (venda == null)
        {
            throw new NotFoundException($"Venda com ID {id} não encontrada.");
        }

        if (venda.StatusVenda != StatusVenda.Rascunho)
        {
            throw new BusinessException("Apenas vendas com status 'Rascunho' podem ser editadas.");
        }

        if (string.IsNullOrWhiteSpace(dto.FormaPagamento))
        {
            throw new BusinessException("A forma de pagamento é obrigatória.");
        }

        if (dto.Itens == null || !dto.Itens.Any())
        {
            throw new BusinessException("A venda deve conter pelo menos um item.");
        }

        if (dto.ClienteId.HasValue)
        {
            var clienteExiste = await _unitOfWork.Clientes.Query().AnyAsync(c => c.Id == dto.ClienteId.Value, cancellationToken);
            if (!clienteExiste)
            {
                throw new BusinessException("O cliente informado não existe ou está inativo.");
            }
        }

        venda.ClienteId = dto.ClienteId;
        venda.FormaPagamento = dto.FormaPagamento.Trim();
        if (dto.DataVenda.HasValue)
        {
            venda.DataVenda = DateTime.SpecifyKind(dto.DataVenda.Value, DateTimeKind.Utc);
        }

        foreach (var oldItem in venda.Itens.ToList())
        {
            _unitOfWork.ItensVenda.Delete(oldItem);
        }
        venda.Itens.Clear();

        var produtoIds = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var variacaoIds = dto.Itens.Select(i => i.VariacaoProdutoId).Distinct().ToList();

        var produtos = await _unitOfWork.Produtos.Query()
            .Where(p => produtoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var variacoes = await _unitOfWork.VariacoesProduto.Query()
            .Where(v => variacaoIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        decimal valorTotal = 0;

        foreach (var itemDto in dto.Itens)
        {
            if (itemDto.Quantidade <= 0)
            {
                throw new BusinessException("A quantidade de cada item deve ser maior que zero.");
            }

            if (!produtos.TryGetValue(itemDto.ProdutoId, out var produto))
            {
                throw new BusinessException($"Produto com ID {itemDto.ProdutoId} não encontrado.");
            }

            if (!variacoes.TryGetValue(itemDto.VariacaoProdutoId, out var variacao))
            {
                throw new BusinessException($"Variação com ID {itemDto.VariacaoProdutoId} não encontrada.");
            }

            if (variacao.ProdutoId != produto.Id)
            {
                throw new BusinessException($"A variação ID {itemDto.VariacaoProdutoId} não pertence ao produto '{produto.Nome}'.");
            }

            var subtotal = produto.ValorVenda * itemDto.Quantidade;
            valorTotal += subtotal;

            venda.Itens.Add(new ItemVenda
            {
                VendaId = venda.Id,
                ProdutoId = produto.Id,
                VariacaoProdutoId = variacao.Id,
                Quantidade = itemDto.Quantidade,
                ValorUnitario = produto.ValorVenda,
                Subtotal = subtotal,
                Status = Status.Ativo,
                CreatedAt = DateTime.UtcNow
            });
        }

        venda.ValorTotal = valorTotal;
        _unitOfWork.Vendas.Update(venda);
        await _unitOfWork.CommitAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<VendaDto> FinalizarAsync(int id, CancellationToken cancellationToken = default)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var venda = await _unitOfWork.Vendas.Query()
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                        .ThenInclude(p => p!.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

            if (venda == null)
            {
                throw new NotFoundException($"Venda com ID {id} não encontrada.");
            }

            if (venda.StatusVenda == StatusVenda.Finalizada)
            {
                throw new BusinessException("Esta venda já foi finalizada anteriormente.");
            }

            if (venda.StatusVenda == StatusVenda.Cancelada)
            {
                throw new BusinessException("Não é possível finalizar uma venda cancelada.");
            }

            if (!venda.Itens.Any())
            {
                throw new BusinessException("A venda não possui itens para ser finalizada.");
            }

            var variacaoIds = venda.Itens.Select(i => i.VariacaoProdutoId).Distinct().OrderBy(vId => vId).ToList();
            var variacoesList = await _unitOfWork.VariacoesProduto.GetByIdsForUpdateAsync(variacaoIds, cancellationToken);
            var variacoes = variacoesList.ToDictionary(v => v.Id);

            var totalQuantityPerVariation = venda.Itens
                .GroupBy(i => i.VariacaoProdutoId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));

            decimal novoTotal = 0;

            foreach (var item in venda.Itens)
            {
                var produto = item.Produto ?? await _unitOfWork.Produtos.Query()
                    .Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.Id == item.ProdutoId, cancellationToken);

                if (produto == null)
                {
                    throw new BusinessException($"Produto ID {item.ProdutoId} não encontrado.");
                }

                if (produto.Status != Status.Ativo)
                {
                    throw new BusinessException($"O produto '{produto.Nome}' está inativo e não pode ser vendido.");
                }

                if (produto.Categoria == null || produto.Categoria.Status != Status.Ativo)
                {
                    throw new BusinessException($"A categoria do produto '{produto.Nome}' está inativa.");
                }

                if (!variacoes.TryGetValue(item.VariacaoProdutoId, out var variacao))
                {
                    throw new BusinessException($"Variação ID {item.VariacaoProdutoId} do produto '{produto.Nome}' não encontrada.");
                }

                if (variacao.ProdutoId != produto.Id)
                {
                    throw new BusinessException($"A variação '{variacao.Tamanho}/{variacao.Cor}' não pertence ao produto '{produto.Nome}'.");
                }

                if (variacao.Status != Status.Ativo)
                {
                    throw new BusinessException($"A variação '{variacao.Tamanho}/{variacao.Cor}' do produto '{produto.Nome}' está inativa e não pode ser vendida.");
                }

                item.ValorUnitario = produto.ValorVenda;
                item.Subtotal = item.ValorUnitario * item.Quantidade;
                novoTotal += item.Subtotal;

                var movimentacao = new MovimentacaoEstoque
                {
                    ProdutoId = produto.Id,
                    VariacaoProdutoId = variacao.Id,
                    Tipo = TipoMovimentacaoEstoque.Saida,
                    Quantidade = item.Quantidade,
                    Motivo = null,
                    DataMovimentacao = venda.DataVenda,
                    Status = Status.Ativo,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.MovimentacoesEstoque.AddAsync(movimentacao, cancellationToken);
            }

            foreach (var (varId, requestedQty) in totalQuantityPerVariation)
            {
                var variacao = variacoes[varId];
                var produtoNome = venda.Itens.FirstOrDefault(i => i.VariacaoProdutoId == varId)?.Produto?.Nome ?? "Produto";

                if (variacao.QuantidadeEstoque < requestedQty)
                {
                    throw new BusinessException($"Estoque insuficiente para a variação '{variacao.Tamanho}/{variacao.Cor}' do produto '{produtoNome}'. Disponível: {variacao.QuantidadeEstoque}, Solicitado: {requestedQty}.");
                }

                variacao.QuantidadeEstoque -= requestedQty;
                _unitOfWork.VariacoesProduto.Update(variacao);
            }

            venda.ValorTotal = novoTotal;
            venda.StatusVenda = StatusVenda.Finalizada;

            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<VendaDto> CancelarAsync(int id, string? motivo = null, CancellationToken cancellationToken = default)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var venda = await _unitOfWork.Vendas.Query()
                .Include(v => v.Itens)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

            if (venda == null)
            {
                throw new NotFoundException($"Venda com ID {id} não encontrada.");
            }

            if (venda.StatusVenda == StatusVenda.Cancelada)
            {
                throw new BusinessException("Esta venda já está cancelada.");
            }

            var motivoTrimmed = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

            if (venda.StatusVenda == StatusVenda.Rascunho)
            {
                venda.StatusVenda = StatusVenda.Cancelada;
                venda.MotivoCancelamento = motivoTrimmed;
                _unitOfWork.Vendas.Update(venda);
                await _unitOfWork.CommitAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return await GetByIdAsync(id, cancellationToken);
            }

            var variacaoIds = venda.Itens.Select(i => i.VariacaoProdutoId).Distinct().OrderBy(vId => vId).ToList();
            var variacoesList = await _unitOfWork.VariacoesProduto.GetByIdsForUpdateAsync(variacaoIds, cancellationToken);
            var variacoes = variacoesList.ToDictionary(v => v.Id);

            foreach (var item in venda.Itens)
            {
                if (variacoes.TryGetValue(item.VariacaoProdutoId, out var variacao))
                {
                    variacao.QuantidadeEstoque += item.Quantidade;
                    _unitOfWork.VariacoesProduto.Update(variacao);

                    var movimentacao = new MovimentacaoEstoque
                    {
                        ProdutoId = item.ProdutoId,
                        VariacaoProdutoId = variacao.Id,
                        Tipo = TipoMovimentacaoEstoque.EstornoVenda,
                        Quantidade = item.Quantidade,
                        Motivo = motivoTrimmed,
                        DataMovimentacao = DateTime.UtcNow,
                        Status = Status.Ativo,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.MovimentacoesEstoque.AddAsync(movimentacao, cancellationToken);
                }
            }

            venda.StatusVenda = StatusVenda.Cancelada;
            venda.MotivoCancelamento = motivoTrimmed;
            _unitOfWork.Vendas.Update(venda);

            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteRascunhoAsync(int id, CancellationToken cancellationToken = default)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id, cancellationToken);
        if (venda == null)
        {
            throw new NotFoundException($"Venda com ID {id} não encontrada.");
        }

        if (venda.StatusVenda != StatusVenda.Rascunho)
        {
            throw new BusinessException("Apenas vendas com status 'Rascunho' podem ser excluídas.");
        }

        venda.Status = Status.Deletado;
        _unitOfWork.Vendas.Update(venda);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    private static VendaDto MapToDto(Venda v) => new()
    {
        Id = v.Id,
        ClienteId = v.ClienteId,
        ClienteNome = v.Cliente?.Nome,
        ClienteTelefone = v.Cliente?.Telefone,
        DataVenda = v.DataVenda,
        FormaPagamento = v.FormaPagamento,
        ValorTotal = v.ValorTotal,
        StatusVenda = v.StatusVenda,
        MotivoCancelamento = v.MotivoCancelamento,
        Status = v.Status,
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt,
        Itens = v.Itens.Select(i => new ItemVendaDto
        {
            Id = i.Id,
            ProdutoId = i.ProdutoId,
            ProdutoNome = i.Produto?.Nome ?? string.Empty,
            ProdutoFoto = i.Produto?.Foto,
            VariacaoProdutoId = i.VariacaoProdutoId,
            Tamanho = i.VariacaoProduto?.Tamanho,
            Cor = i.VariacaoProduto?.Cor,
            Quantidade = i.Quantidade,
            ValorUnitario = i.ValorUnitario,
            Subtotal = i.Subtotal
        }).ToList()
    };
}
