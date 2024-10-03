using MrHell.Data.Models;
using MrHell.Items;
using MrHell.Items.Base;
using PixelPilot.PixelGameClient.Messages.Received;
using PixelPilot.PixelGameClient.Players.Basic;

namespace MrHell.Players;

/// <summary>
/// Extends the base <see cref="Player"/> with some additional values that are game specific.
/// Also adds a <see cref="Profile"/> that is used to save Player data.
/// </summary>
public class HellPlayer : Player
{
    public DateTime LastMovement { get; set; }
    public bool IsMarkedAfk { get; set; }
    public bool IsForcedAfk { get; set; }
    public bool IsAfk => IsForcedAfk || IsMarkedAfk;

    public PlayerRole Role => Profile?.Role ?? PlayerRole.Default;

    public bool IsAvailable => !IsAfk & !Godmode & !Modmode && Profile != null;

    public PlayerProfile? Profile;

    public HellItem? SelectedItem { get; set; } = null;
    public int CleanDown { get; set; } = 0;
    public DateTime LastItemUse { get; set; } = DateTime.UtcNow;
    
    public HellPlayer(PlayerJoinPacket packet) : base(packet)
    {
        
    }
}