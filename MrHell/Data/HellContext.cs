using Microsoft.EntityFrameworkCore;
using MrHell.Data.Models;

namespace MrHell.Data;

public class HellContext : DbContext
{
    public DbSet<PlayerProfile> Profiles { get; set; }

    public string DbPath { get; }

    public HellContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "hell-bot-data.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlayerProfileTypeConfiguration).Assembly);
    }
}