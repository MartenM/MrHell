using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.GameCommands;

public class AfkCommand : ChatCommand
{
    public AfkCommand() : base("afk", "Be marked as AFK", null)
    {
        
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        var player = (HellPlayer) sender.Player;

        player.IsMarkedAfk = !player.IsMarkedAfk;
        if (player.IsMarkedAfk)
        {
            sender.SendMessage("You are now marked as AFK. Use the command again to start playing again.");
        }
        else
        {
            sender.SendMessage("You are no longer AFK.");
        }

        return Task.CompletedTask;
    }
}