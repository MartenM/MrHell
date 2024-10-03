using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.AdminCommands;

public class StartCommand : ChatCommand
{
    private HellBot _bot;
    
    public StartCommand(HellBot bot) : base("start", "Force starts a game.", "game.start")
    {
        _bot = bot;
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        if (_bot.OngoingGame)
        {
            sender.SendMessage("A game is still ongoing it. Cancel it before starting a new one.");
            return Task.CompletedTask;
        }
        
        _ = _bot.StartGameSafe();
        sender.SendMessage("Force started the game.");
        return Task.CompletedTask;
    }
}