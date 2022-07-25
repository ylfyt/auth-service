using auth_sevice.Src.Models;
using auth_sevice.Src.Types;
using Microsoft.EntityFrameworkCore;

namespace auth_sevice.Src.Data
{
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

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

    public DbSet<User> Users { get; set; } = null!;

  }
}