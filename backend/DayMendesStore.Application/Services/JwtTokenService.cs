using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DayMendesStore.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public (string token, DateTime expiration) GenerateToken(Loja loja)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var expiration = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, loja.Id.ToString()),
            new Claim(ClaimTypes.Email, loja.Email),
            new Claim(ClaimTypes.Name, loja.Nome),
            new Claim("LojaId", loja.Id.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiration);
    }
}
