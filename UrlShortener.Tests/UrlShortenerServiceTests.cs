using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using UrlShortener.Api.Data;
using UrlShortener.Api.Services;
using Xunit;

public class UrlShortenerServiceTests
{
    private AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(opts);
    }

    private UrlShortenerService CreateService(AppDbContext db)
    {
        var mockRedis  = new Mock<IConnectionMultiplexer>();
        var mockDb     = new Mock<IDatabase>();
        mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        return new UrlShortenerService(db, mockRedis.Object);
    }

    [Fact]
    public async Task ShortenAsync_ValidUrl_ReturnsShortCode()
    {
        using var db = CreateDb();
        var svc    = CreateService(db);
        var result = await svc.ShortenAsync("https://example.com/very/long/path");
        Assert.NotEmpty(result.ShortCode);
        Assert.Equal(7, result.ShortCode.Length);
    }

    [Fact]
    public async Task ResolveAsync_UnknownCode_ReturnsNull()
    {
        using var db = CreateDb();
        var svc    = CreateService(db);
        var result = await svc.ResolveAsync("XXXXXXX");
        Assert.Null(result);
    }
}