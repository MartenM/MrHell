using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.GameCommands;

public class CancelCommand : ChatCommand
{
    private HellBot _bot;
    
    public CancelCommand(HellBot bot) : base("cancel", "Cancels a round.", "game.cancel")
    {
        _bot = bot;
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        if (!_bot.OngoingGame)
        {
            sender.SendMessage("No game is ongoing. Cannot cancel.");
            return Task.CompletedTask;
        }

        _ = _bot.CancelGame();
        sender.SendMessage("Game is being cancelled.");
        return Task.CompletedTask;
    }
}