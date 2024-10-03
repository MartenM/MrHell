using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.AdminCommands;

public class AutoCommand : ChatCommand
{
    private HellBot _bot;

    public AutoCommand(HellBot bot) : base("auto", "Toggle automatic mode.", "game.auto")
    {
        _bot = bot;
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        _bot.ToggleAutomatic();
        sender.SendMessage($"Automatic mode: {_bot.AutomaticStart}");
        return Task.CompletedTask;
    }
}