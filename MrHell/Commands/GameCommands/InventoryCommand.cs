using MrHell.Items;
using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.GameCommands;

public class InventoryCommand : ChatCommand
{
    public InventoryCommand() : base("inventory", "Check your inventory", null)
    {
        Aliases.Add("inv");
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        var player = (HellPlayer) sender.Player;
        
        if (player!.Profile!.Inventory.Count == 0)
        {
            sender.SendMessage("Your inventory is empty.");
            return Task.CompletedTask;
        }
        
        var inventory = player.Profile.Inventory
            .GroupBy(i => i)
            .Select(g => new {Item = ItemManager.Instance.GetItem(g.Key), Count = g.Count()});
        
        var message = string.Join(", ", inventory.Select(i => $"{i.Item.Name} ({i.Count})"));
        sender.SendMessage($"INV: {message}");
        return Task.CompletedTask;
    }
}