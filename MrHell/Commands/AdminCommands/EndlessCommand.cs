using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.AdminCommands;

public class EndlessCommand : ChatCommand
{
    private HellBot _bot;
    
    public EndlessCommand(HellBot bot) : base("endless", "Toggle endless mode", "game.endless")
    {
        _bot = bot;
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        _bot.Endless = !_bot.Endless;
        sender.SendMessage($"Changed endless to: {_bot.Endless}");
        return Task.CompletedTask;
    }
}