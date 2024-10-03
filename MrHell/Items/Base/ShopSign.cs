using System.Drawing;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Base;

public class ShopSign
{
    public ShopSign(Point location)
    {
        Location = location;
    }

    public Point Location { get; private set; }
    public ShopOffer? Offer { get; set; }

    public List<IPlacedBlock> ShopBlocks()
    {
        var blocks = new List<IPlacedBlock>();
        
        if (Offer != null)
        {
            var signText = $"**{Offer.Item.Name}**\n" +
                       $"*{Offer.Item.Description}*\n" +
                       $"\n+" +
                       $"{Offer.Coins} coins\n" +
                       $"Press __down__ to buy.";
            
            blocks.Add(new PlacedBlock(Location.X, Location.Y, WorldLayer.Foreground, new SignBlock(PixelBlock.SignNormal, signText)));
            blocks.Add(new PlacedBlock(Location.X, Location.Y + 2, WorldLayer.Foreground, Offer.ShopBlock));
        }
        else
        {
            var signText = $"__No offer available__";
            blocks.Add(new PlacedBlock(Location.X, Location.Y, WorldLayer.Foreground, new SignBlock(PixelBlock.SignRed, signText)));
            blocks.Add(new PlacedBlock(Location.X, Location.Y + 2, WorldLayer.Foreground, new BasicBlock(PixelBlock.BrickBlack)));
        }

        return blocks;
    }
}