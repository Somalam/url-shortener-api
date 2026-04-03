using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public class UrlShortenerService(AppDbContext db, IConnectionMultiplexer redis)
{
    private readonly IDatabase _cache = redis.GetDatabase();
    private const int CodeLength = 7;

    public async Task<ShortenedUrl> ShortenAsync(string originalUrl)
    {
        var code = GenerateCode();
        var entity = new ShortenedUrl { OriginalUrl = originalUrl, ShortCode = code };
        db.ShortenedUrls.Add(entity);
        await db.SaveChangesAsync();
        await _cache.StringSetAsync(code, originalUrl, TimeSpan.FromHours(24));
        return entity;
    }

    public async Task<string?> ResolveAsync(string code)
    {
        // 1. Check Redis cache first (Merit: caching)
        var cached = await _cache.StringGetAsync(code);
        if (cached.HasValue) return cached!;

        // 2. Fall back to database
        var entity = await db.ShortenedUrls
            .FirstOrDefaultAsync(u => u.ShortCode == code);
        if (entity is null) return null;

        entity.HitCount++;
        await db.SaveChangesAsync();
        await _cache.StringSetAsync(code, entity.OriginalUrl, TimeSpan.FromHours(24));
        return entity.OriginalUrl;
    }

    private static string GenerateCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, CodeLength)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}