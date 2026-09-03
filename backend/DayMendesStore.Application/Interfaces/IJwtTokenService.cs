using DayMendesStore.Domain.Entities;

namespace DayMendesStore.Application.Interfaces;

public interface IJwtTokenService
{
    (string token, DateTime expiration) GenerateToken(Loja loja);
}
