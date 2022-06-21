using Microsoft.EntityFrameworkCore;
using QuickGridSamples.Core.Models;

namespace QuickGridSamples.Server.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Country> Countries { get; set; } = default!;

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>().HasKey(x => x.Code);
        modelBuilder.Entity<Country>().OwnsOne(x => x.Medals);
    }
}
