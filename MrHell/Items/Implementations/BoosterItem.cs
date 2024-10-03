using MrHell.Items.Base;
using MrHell.Players;
using MrHell.Util;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Implementations;

public class BoosterItem : HellItem
{
    public BoosterItem() : base("item.boosts", "Boosts", "Lets start jumping!")
    {
        
    }

    public override async Task Execute(HellPlayer player, IHellApi api)
    {
        int x = (int) player.X / 16;
        int y = Platform.Y;
        
        for (int i = -1; i < 2; i++)
        {
            api.PlaceBlock(new PlacedBlock(x + i, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.GenericYellowFaceSmile)));
        }

        await Task.Delay(TimeSpan.FromSeconds(2));
        
        for (int i = -1; i < 2; i++)
        {
            api.PlaceBlock(new PlacedBlock(x + i, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.BoostUp)));
        }
        
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        for (int i = -1; i < 2; i++)
        {
            api.PlaceBlock(new PlacedBlock(x + i, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.BrickRed)));
        }
    }
}