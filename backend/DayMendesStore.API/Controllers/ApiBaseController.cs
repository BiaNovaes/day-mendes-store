using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class ApiBaseController : ControllerBase
{
    protected int? GetAuthenticatedLojaId()
    {
        var claim = User.FindFirst("LojaId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var id))
        {
            return id;
        }
        return null;
    }

    protected string? GetAuthenticatedEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }
}
