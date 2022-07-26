using auth_sevice.src.Models;
using auth_sevice.src.Types;
using Microsoft.EntityFrameworkCore;

namespace auth_sevice.src.Data
{
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public override int SaveChanges()
    {
      UpdateTime();
      return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      UpdateTime();
      return await base.SaveChangesAsync();
    }

    private void UpdateTime()
    {
      var entries = ChangeTracker
          .Entries()
          .Where(e => e.Entity is BaseModel && (
                  e.State == EntityState.Added
                  || e.State == EntityState.Modified));
      var now = DateTime.UtcNow;
      foreach (var entityEntry in entries)
      {
        ((BaseModel)entityEntry.Entity).UpdatedAt = now;

        if (entityEntry.State == EntityState.Added)
        {
          ((BaseModel)entityEntry.Entity).CreatedAt = now;
        }
      }
    }
  }
}