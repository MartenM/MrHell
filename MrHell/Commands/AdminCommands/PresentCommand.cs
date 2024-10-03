using MrHell.Players;
using MrHell.Util;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;
using PixelPilot.PixelGameClient;

namespace MrHell.Commands.AdminCommands;

public class PresentCommand : ChatCommand
{
    private HellPlayerManager _playerManager;
    private PixelPilotClient _client;
    
    public PresentCommand(HellPlayerManager playerManager, PixelPilotClient client) : base("present", "Give a present to all players.", "admin.present")
    {
        _playerManager = playerManager;
        _client = client;
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        int coins = 20;
        if (args.Length >= 1)
        {
            int.TryParse(args[0], out coins);
        }
        
        foreach (var player in _playerManager.Players.Where(p => !p.IsAfk))
        {
            var rewardedCoins = coins + HellRandom.Next(20);
            _client.SendPm(player.Username, $"You got present from {sender.Player.Username}. It contained {rewardedCoins} coins.");
            player.Profile!.Coins += rewardedCoins;
        }
        
        return Task.CompletedTask;
    }
}