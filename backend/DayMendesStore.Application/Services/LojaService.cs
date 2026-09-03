using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class LojaService : ILojaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;

    public LojaService(IUnitOfWork unitOfWork, IPasswordHasherService passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<LojaDto> GetLojaAsync(CancellationToken cancellationToken = default)
    {
        var loja = await _unitOfWork.Lojas.Query().FirstOrDefaultAsync(cancellationToken);
        if (loja == null)
        {
            throw new NotFoundException("Loja não encontrada. É necessário realizar a configuração inicial.");
        }

        return MapToDto(loja);
    }

    public async Task<LojaDto> UpdateLojaAsync(LojaUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        var loja = await _unitOfWork.Lojas.Query().FirstOrDefaultAsync(cancellationToken);
        if (loja == null)
        {
            throw new NotFoundException("Loja não encontrada.");
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new BusinessException("O nome da loja é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BusinessException("O e-mail da loja é obrigatório.");

        loja.Nome = request.Nome;
        loja.Email = request.Email;
        loja.Foto = request.Foto ?? loja.Foto;
        loja.Cnpj = request.Cnpj;
        loja.CorPrimaria = request.CorPrimaria;
        loja.CorSecundaria = request.CorSecundaria;

        _unitOfWork.Lojas.Update(loja);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToDto(loja);
    }

    public async Task AlterarSenhaAsync(AlterarSenhaDto request, CancellationToken cancellationToken = default)
    {
        var loja = await _unitOfWork.Lojas.Query().FirstOrDefaultAsync(cancellationToken);
        if (loja == null)
        {
            throw new NotFoundException("Loja não encontrada.");
        }

        if (string.IsNullOrWhiteSpace(request.SenhaAtual) || string.IsNullOrWhiteSpace(request.NovaSenha))
        {
            throw new BusinessException("A senha atual e a nova senha são obrigatórias.");
        }

        if (!_passwordHasher.VerifyPassword(request.SenhaAtual, loja.Senha))
        {
            throw new BusinessException("A senha atual está incorreta.");
        }

        if (request.NovaSenha.Length < 6)
        {
            throw new BusinessException("A nova senha deve ter pelo menos 6 caracteres.");
        }

        loja.Senha = _passwordHasher.HashPassword(request.NovaSenha);
        _unitOfWork.Lojas.Update(loja);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task<string> AtualizarFotoAsync(string fotoUrl, CancellationToken cancellationToken = default)
    {
        var loja = await _unitOfWork.Lojas.Query().FirstOrDefaultAsync(cancellationToken);
        if (loja == null)
        {
            throw new NotFoundException("Loja não encontrada.");
        }

        loja.Foto = fotoUrl;
        _unitOfWork.Lojas.Update(loja);
        await _unitOfWork.CommitAsync(cancellationToken);

        return fotoUrl;
    }

    private static LojaDto MapToDto(Loja loja) => new()
    {
        Id = loja.Id,
        Nome = loja.Nome,
        Foto = loja.Foto,
        Email = loja.Email,
        Cnpj = loja.Cnpj,
        CorPrimaria = loja.CorPrimaria,
        CorSecundaria = loja.CorSecundaria,
        Status = loja.Status,
        CreatedAt = loja.CreatedAt,
        UpdatedAt = loja.UpdatedAt
    };
}
