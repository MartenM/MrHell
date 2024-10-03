using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.AdminCommands;

public class SpawnCoinsCommand : ChatCommand
{
    public const int Amount = 100;
    
    public SpawnCoinsCommand() : base("spawnCoins", "Give the executor coins.", "admin.spawnCoins")
    {
        
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        var player = (HellPlayer) sender.Player;
        player.Profile!.Coins += Amount;
        
        sender.SendMessage($"You have been given {Amount} coins.");
        return Task.CompletedTask;
    }
}