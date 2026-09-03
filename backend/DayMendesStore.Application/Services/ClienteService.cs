using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<ClienteDto>> GetPagedAsync(ClienteFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Clientes.Query();

        if (!string.IsNullOrWhiteSpace(filtro.TermoBusca))
        {
            var termo = filtro.TermoBusca.Trim().ToLower();
            query = query.Where(c => c.Nome.ToLower().Contains(termo)
                                  || (c.Apelido != null && c.Apelido.ToLower().Contains(termo))
                                  || (c.Email != null && c.Email.ToLower().Contains(termo))
                                  || (c.Telefone != null && c.Telefone.ToLower().Contains(termo)));
        }

        if (filtro.Status.HasValue)
        {
            query = query.Where(c => c.Status == filtro.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var clientes = await query
            .OrderBy(c => c.Nome)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync(cancellationToken);

        var clienteIds = clientes.Select(c => c.Id).ToList();

        var vendasAgrupadas = await _unitOfWork.Vendas.Query()
            .Where(v => v.ClienteId != null && clienteIds.Contains(v.ClienteId.Value) && v.StatusVenda == StatusVenda.Finalizada)
            .GroupBy(v => v.ClienteId!.Value)
            .Select(g => new
            {
                ClienteId = g.Key,
                TotalCompras = g.Count(),
                ValorTotal = g.Sum(v => v.ValorTotal),
                UltimaCompra = g.Max(v => v.DataVenda)
            })
            .ToDictionaryAsync(g => g.ClienteId, cancellationToken);

        var items = clientes.Select(c =>
        {
            vendasAgrupadas.TryGetValue(c.Id, out var dadosVenda);
            return new ClienteDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Apelido = c.Apelido,
                Email = c.Email,
                Telefone = c.Telefone,
                Endereco = c.Endereco,
                Status = c.Status,
                TotalCompras = dadosVenda?.TotalCompras ?? 0,
                ValorTotalComprado = dadosVenda?.ValorTotal ?? 0,
                UltimaCompraEm = dadosVenda?.UltimaCompra,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }).ToList();

        return new PagedResultDto<ClienteDto>(items, totalCount, filtro.Page, filtro.PageSize);
    }

    public async Task<IReadOnlyList<ClienteDto>> GetAllAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Clientes.Query()
            .Where(c => c.Status == Status.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Apelido = c.Apelido,
                Email = c.Email,
                Telefone = c.Telefone,
                Endereco = c.Endereco,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ClienteDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new NotFoundException($"Cliente com ID {id} não encontrado.");
        }

        var dadosVenda = await _unitOfWork.Vendas.Query()
            .Where(v => v.ClienteId == id && v.StatusVenda == StatusVenda.Finalizada)
            .GroupBy(v => v.ClienteId)
            .Select(g => new
            {
                TotalCompras = g.Count(),
                ValorTotal = g.Sum(v => v.ValorTotal),
                UltimaCompra = g.Max(v => v.DataVenda)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Apelido = cliente.Apelido,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Endereco = cliente.Endereco,
            Status = cliente.Status,
            TotalCompras = dadosVenda?.TotalCompras ?? 0,
            ValorTotalComprado = dadosVenda?.ValorTotal ?? 0,
            UltimaCompraEm = dadosVenda?.UltimaCompra,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt
        };
    }

    public async Task<ClienteDto> CreateAsync(ClienteCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new BusinessException("O nome do cliente é obrigatório.");
        }

        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Apelido = string.IsNullOrWhiteSpace(dto.Apelido) ? null : dto.Apelido.Trim(),
            Email = dto.Email?.Trim(),
            Telefone = dto.Telefone?.Trim(),
            Endereco = dto.Endereco?.Trim(),
            Status = Status.Ativo,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Apelido = cliente.Apelido,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Endereco = cliente.Endereco,
            Status = cliente.Status,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt
        };
    }

    public async Task<ClienteDto> UpdateAsync(int id, ClienteUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new NotFoundException($"Cliente com ID {id} não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new BusinessException("O nome do cliente é obrigatório.");
        }

        cliente.Nome = dto.Nome.Trim();
        cliente.Apelido = string.IsNullOrWhiteSpace(dto.Apelido) ? null : dto.Apelido.Trim();
        cliente.Email = dto.Email?.Trim();
        cliente.Telefone = dto.Telefone?.Trim();
        cliente.Endereco = dto.Endereco?.Trim();

        _unitOfWork.Clientes.Update(cliente);
        await _unitOfWork.CommitAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task InativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new NotFoundException($"Cliente com ID {id} não encontrado.");
        }

        if (cliente.Status == Status.Inativo)
            return;

        cliente.Status = Status.Inativo;
        _unitOfWork.Clientes.Update(cliente);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task ReativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new NotFoundException($"Cliente com ID {id} não encontrado.");
        }

        if (cliente.Status == Status.Ativo)
            return;

        cliente.Status = Status.Ativo;
        _unitOfWork.Clientes.Update(cliente);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        if (cliente == null)
        {
            throw new NotFoundException($"Cliente com ID {id} não encontrado.");
        }

        cliente.Status = Status.Deletado;
        _unitOfWork.Clientes.Update(cliente);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
