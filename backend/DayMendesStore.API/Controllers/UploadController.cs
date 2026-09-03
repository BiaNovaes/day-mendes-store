using DayMendesStore.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class UploadFotoRequest
{
    public required IFormFile File { get; set; }
}

public class UploadController : ApiBaseController
{
    private readonly IWebHostEnvironment _environment;

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("foto")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadFoto([FromForm] UploadFotoRequest request)
    {
        var file = request?.File;
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("Nenhum arquivo enviado.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            throw new BusinessException("Formato de arquivo inválido. Permitidos: JPG, PNG, WEBP.");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsFolder = Path.Combine(webRoot, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/{fileName}";
        return Ok(new { url = fileUrl });
    }
}
