namespace DayMendesStore.Application.DTOs;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DayMendesStore";
    public string Audience { get; set; } = "DayMendesStoreApp";
    public int ExpirationHours { get; set; } = 2;
}
