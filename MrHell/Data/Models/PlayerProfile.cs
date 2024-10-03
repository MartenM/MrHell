using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MrHell.Data.Models;

public class PlayerProfile
{
    public string PixelId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public PlayerRole Role { get; set; } 
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public bool Banned { get; set; }
    public int Wins { get; set; }
    public int Coins { get; set; }
    
    // Items
    public List<string> Inventory = new();
}

public class PlayerProfileTypeConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.HasKey(p => p.PixelId);
        builder.Property(p => p.PixelId)
            .HasMaxLength(16)
            .IsUnicode(false);
        
        builder.HasIndex(p => p.Username);
        builder.Property(p => p.Username)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired();
        
        builder.Property(p => p.FirstSeen)
            .HasDefaultValueSql("now()");
        
        builder.Property(p => p.LastSeen)
            .HasDefaultValueSql("now()");
        
        builder.Property(p => p.Role)
            .HasDefaultValue(PlayerRole.Default)
            .IsRequired();
        
        builder.Property(p => p.Role)
            .HasConversion(
                v => v.ToString(),
                v => (PlayerRole) Enum.Parse(typeof(PlayerRole), v));
    }
}