using Microsoft.EntityFrameworkCore;
using MrHell.Data;
using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.GameCommands;

public class CoinsCommand : ChatCommand
{
    private HellPlayerManager _playerManager;
    
    public CoinsCommand(HellPlayerManager playerManager) : base("coins", "Check the amount of coins", null)
    {
        _playerManager = playerManager;
    }

    public override async Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        // Get self
        if (args.Length == 0)
        {
            var player = (HellPlayer) sender.Player;
            sender.SendMessage($"You have {player.Profile?.Coins ?? 0} coins.");
            return;
        }

        var targetUser = args[0];

        // Get from online
        var onlinePlayer = _playerManager.GetPlayerByUsername(targetUser);
        if (onlinePlayer != null && onlinePlayer.Profile != null)
        {
            sender.SendMessage($"{onlinePlayer.Profile.Username} has {onlinePlayer.Profile.Coins} coins.");
            return;
        }

        // Get from DB
        await using var context = new HellContext();
        var profile = await context.Profiles
            .Where(p => string.Equals(p.Username, targetUser))
            .FirstOrDefaultAsync();

        if (profile == null)
        {
            sender.SendMessage("Could not find a player with that username.");
            return;
        }
            
        sender.SendMessage($"{profile.Username} has {profile.Wins} coins.");
    }
}