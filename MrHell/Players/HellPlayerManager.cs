using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MrHell.Data;
using MrHell.Data.Models;
using PixelPilot.PixelGameClient.Messages.Received;
using PixelPilot.PixelGameClient.Players;

namespace MrHell.Players;

public class HellPlayerManager : PixelPlayerManager<HellPlayer>
{
    public HellPlayerManager()
    {
        OnPlayerLeft += _onPlayerLeft;
    }
    
    protected override HellPlayer CreatePlayer(PlayerJoinPacket join)
    {
        var player = new HellPlayer(join);
        player.LastMovement = DateTime.UtcNow;

        Task.Run(async () =>
        {
            try
            {
                await using (var context = new HellContext())
                {
                    var profile = await context.Profiles
                        .Where(p => p.PixelId == player.AccountId)
                        .FirstOrDefaultAsync();

                    if (profile == null)
                    {
                        profile = new PlayerProfile()
                        {
                            PixelId = player.AccountId,
                            Username = player.Username,
                            FirstSeen = DateTime.UtcNow,
                            LastSeen = DateTime.UtcNow,
                        };

                        await context.Profiles.AddAsync(profile);
                        await context.SaveChangesAsync();
                        
                        _logger.LogInformation("A new profile was created for {USERNAME}", player.Username);
                    }

                    // Update things
                    profile.LastSeen = DateTime.UtcNow;
                    profile.Username = player.Username;
                    
                    player.Profile = profile;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load player profile...");
            }
        });

        return player;
    }

    public async Task SaveAll()
    {
        var tasks = Players
            .Select(_savePlayer)
            .ToList();

        await Task.WhenAll(tasks);
    }
    private void _onPlayerLeft(Object sender, HellPlayer player)
    {
        // Task to save the player to the database on quit.
        _ = _savePlayer(player);
    }

    private async Task _savePlayer(HellPlayer player)
    {
        if (player.Profile == null) return;

        try
        {
            await using (var context = new HellContext())
            {
                context.Attach(player.Profile);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving profile of {USERNAME} on quit.", player.Username);
        }
    }
}