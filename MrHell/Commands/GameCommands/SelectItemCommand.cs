using MrHell.Items;
using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.GameCommands;

public class SelectItemCommand : ChatCommand
{
    public SelectItemCommand() : base("select", "Select an item for use.", null)
    {
        Aliases.Add("s");
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        var player = (HellPlayer) sender.Player;

        if (args.Length < 1)
        {
            if (player.SelectedItem == null)
            {
                sender.SendMessage("Select an item from your inventory by doing !select <name>.");
            }
            else
            {
                sender.SendMessage($"Selected item: {player.SelectedItem.Name}");
            }
            
            return Task.CompletedTask;
        }
        
        // Check if the item is in the inventory.
        var item = ItemManager.Instance.GetItemByName(args[0]);
        if (item == null)
        {
            sender.SendMessage("That item does not exist!");
            return Task.CompletedTask;
        }

        if (!player.Profile?.Inventory.Contains(item.Id) ?? true)
        {
            sender.SendMessage("You do not own that item. Buy it at the shop!");
            return Task.CompletedTask;
        }

        sender.SendMessage($"Selected: {item.Name}. Press down twice to use.");
        player.SelectedItem = item;
        
        
        return Task.CompletedTask;;
    }
}