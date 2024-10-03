using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.AdminCommands;

public class DisconnectCommand : ChatCommand
{
    private HellBot _bot;
    
    public DisconnectCommand(HellBot bot) : base("disconnect", "Disconnect and stop the bot.", "bot.disconnect")
    {
        _bot = bot;
        Aliases.Add("dc");
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        sender.SendMessage("Order received. I will shutdown...");
        _ = _bot.Stop();
        return Task.CompletedTask;
    }
}