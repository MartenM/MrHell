using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Base;

public class ShopOffer
{
    public ShopOffer(HellItem item, PixelBlock shopBlock, int coins)
    {
        Item = item;
        ShopBlock = new BasicBlock(shopBlock);
        Coins = coins;
    }
    
    public ShopOffer(HellItem item, IPixelBlock shopBlock, int coins)
    {
        Item = item;
        ShopBlock = shopBlock;
        Coins = coins;
    }

    public HellItem Item { get; private set; }
    public IPixelBlock ShopBlock { get; private set; }
    public int Coins { get; private set; }
}