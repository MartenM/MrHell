using MrHell.Items;
using PixelPilot.ChatCommands;
using PixelPilot.ChatCommands.Commands;

namespace MrHell.Commands.AdminCommands;

public class ShopRefreshCommand : ChatCommand
{
    private ShopManager _shopManager;
    
    public ShopRefreshCommand(ShopManager shopManager) : base("shopRefresh", "Refresh the shop.", "shop.refresh")
    {
        _shopManager = shopManager;
    }

    public override Task ExecuteCommand(ICommandSender sender, string fullCommand, string[] args)
    {
        _shopManager.RepopulateShopSigns();
        sender.SendMessage("Shops have been refreshed.");
        return Task.CompletedTask;
    }
}