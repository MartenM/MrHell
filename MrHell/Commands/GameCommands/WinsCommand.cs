using Microsoft.EntityFrameworkCore;
using MrHell.Data;
using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.GameCommands;

public class WinsCommand : ChatCommand
{
    public WinsCommand() : base("wins", "Check the amount of wins of a person or player.", null)
    {
        
    }

    public override async Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        if (args.Length == 0)
        {
            var player = (HellPlayer)sender.Player;
            sender.SendMessage($"You have won {player.Profile?.Wins ?? 0} times.");
            return;
        }

        var targetUser = args[0];
        await using (var context = new HellContext())
        {
            var profile = await context.Profiles
                .Where(p => string.Equals(p.Username, targetUser))
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                sender.SendMessage("Could not find a player with that username.");
                return;
            }
            
            sender.SendMessage($"{profile.Username} has won {profile.Wins} times.");
            return;
        }
    }
}