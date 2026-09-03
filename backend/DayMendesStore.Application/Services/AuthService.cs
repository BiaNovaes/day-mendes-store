using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Exceptions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            throw new BusinessException("E-mail e senha são obrigatórios.");
        }

        var loja = await _unitOfWork.Lojas.Query()
            .FirstOrDefaultAsync(l => l.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (loja == null || !_passwordHasher.VerifyPassword(request.Senha, loja.Senha))
        {
            throw new UnauthorizedException("E-mail ou senha inválidos.");
        }

        if (loja.Status != Status.Ativo)
        {
            throw new BusinessException("A conta da loja está inativa.");
        }

        var (token, expiration) = _jwtTokenService.GenerateToken(loja);

        return new LoginResponseDto
        {
            Token = token,
            ExpiraEm = expiration,
            Loja = MapToDto(loja)
        };
    }

    public async Task<LojaDto> SetupLojaAsync(SetupLojaDto request, CancellationToken cancellationToken = default)
    {
        var existingLoja = await _unitOfWork.Lojas.Query().FirstOrDefaultAsync(cancellationToken);
        if (existingLoja != null)
        {
            throw new BusinessException("A loja já foi configurada no sistema.");
        }

        var loja = new Loja
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = _passwordHasher.HashPassword(request.Senha),
            Cnpj = request.Cnpj,
            CorPrimaria = request.CorPrimaria,
            CorSecundaria = request.CorSecundaria,
            Status = Status.Ativo,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Lojas.AddAsync(loja, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToDto(loja);
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
