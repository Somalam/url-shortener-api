using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ShortenedUrl>()
          .HasIndex(e => e.ShortCode).IsUnique();
    }
}