using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<CategoriaDto>> GetPagedAsync(PaginationParamsDto pagination, Status? status = null, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Categorias.Query();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                Status = c.Status,
                TotalProdutos = c.Produtos.Count(p => p.Status != Status.Deletado),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CategoriaDto>(items, totalCount, pagination.Page, pagination.PageSize);
    }

    public async Task<IReadOnlyList<CategoriaDto>> GetAllAtivasAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Categorias.Query()
            .Where(c => c.Status == Status.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                Status = c.Status,
                TotalProdutos = c.Produtos.Count(p => p.Status == Status.Ativo),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.Query()
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID {id} não encontrada.");
        }

        return new CategoriaDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Status = categoria.Status,
            TotalProdutos = categoria.Produtos.Count(p => p.Status != Status.Deletado),
            CreatedAt = categoria.CreatedAt,
            UpdatedAt = categoria.UpdatedAt
        };
    }

    public async Task<CategoriaDto> CreateAsync(CategoriaCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new BusinessException("O nome da categoria é obrigatório.");
        }

        var existe = await _unitOfWork.Categorias.Query()
            .AnyAsync(c => c.Nome.ToLower() == dto.Nome.Trim().ToLower(), cancellationToken);

        if (existe)
        {
            throw new BusinessException($"Já existe uma categoria cadastrada com o nome '{dto.Nome.Trim()}'.");
        }

        var categoria = new Categoria
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            Status = Status.Ativo,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Categorias.AddAsync(categoria, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CategoriaDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Status = categoria.Status,
            TotalProdutos = 0,
            CreatedAt = categoria.CreatedAt,
            UpdatedAt = categoria.UpdatedAt
        };
    }

    public async Task<CategoriaDto> UpdateAsync(int id, CategoriaUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, cancellationToken);
        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID {id} não encontrada.");
        }

        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new BusinessException("O nome da categoria é obrigatório.");
        }

        var existeOutra = await _unitOfWork.Categorias.Query()
            .AnyAsync(c => c.Id != id && c.Nome.ToLower() == dto.Nome.Trim().ToLower(), cancellationToken);

        if (existeOutra)
        {
            throw new BusinessException($"Já existe outra categoria com o nome '{dto.Nome.Trim()}'.");
        }

        categoria.Nome = dto.Nome.Trim();
        categoria.Descricao = dto.Descricao?.Trim();

        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.CommitAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task InativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, cancellationToken);
        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID {id} não encontrada.");
        }

        if (categoria.Status == Status.Inativo)
        {
            return;
        }

        categoria.Status = Status.Inativo;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task ReativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, cancellationToken);
        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID {id} não encontrada.");
        }

        if (categoria.Status == Status.Ativo)
        {
            return;
        }

        categoria.Status = Status.Ativo;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.Query()
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID {id} não encontrada.");
        }

        var temProdutosVinculados = categoria.Produtos.Any(p => p.Status != Status.Deletado);
        if (temProdutosVinculados)
        {
            throw new BusinessException("Não é possível excluir uma categoria que possui produtos vinculados. Inative-a ou remova os produtos antes.");
        }

        categoria.Status = Status.Deletado;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
