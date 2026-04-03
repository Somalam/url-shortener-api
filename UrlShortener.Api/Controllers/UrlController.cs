using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlController(UrlShortenerService svc) : ControllerBase
{
    [HttpPost("shorten")]
    public async Task<IActionResult> Shorten([FromBody] ShortenRequest req)
    {
        if (!Uri.TryCreate(req.Url, UriKind.Absolute, out _))
            return BadRequest(new { error = "Invalid URL format." });

        var result = await svc.ShortenAsync(req.Url);
        var shortUrl = $"{Request.Scheme}://{Request.Host}/r/{result.ShortCode}";
        return Ok(new { shortUrl, result.ShortCode, result.CreatedAt });
    }

    [HttpGet("/r/{code}")]
    public async Task<IActionResult> Redirect(string code)
    {
        var url = await svc.ResolveAsync(code);
        return url is null ? NotFound() : Redirect(url);
    }

    [HttpGet("stats/{code}")]
    public async Task<IActionResult> Stats(string code)
    {
        // Returns hit count and metadata
        var url = await svc.ResolveAsync(code);
        return url is null ? NotFound() : Ok(new { code, resolves_to = url });
    }
}

public record ShortenRequest(string Url);