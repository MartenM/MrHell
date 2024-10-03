using MrHell.Items.Base;
using MrHell.Players;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Effects;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Implementations;

public class CurseItem : HellItem
{
    public CurseItem() : base("item.curse", "Curse", "Spawn a curse block on you.")
    {
        
    }

    public override async Task Execute(HellPlayer player, IHellApi api)
    {
        var x = (int)player.X / 16;
        var y = (int)player.Y / 16;
        api.PlaceBlock( new PlacedBlock(x, y - 1, WorldLayer.Foreground, new TimedEffectBlock(PixelBlock.EffectsCurse, 10)));
        api.PlaceBlock( new PlacedBlock(x, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.GravityUp)));

        await Task.Delay(500);
        
        api.PlaceBlock( new PlacedBlock(x, y - 1, WorldLayer.Foreground,  new BasicBlock(PixelBlock.Empty)));
        api.PlaceBlock( new PlacedBlock(x, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.Empty)));

    }
}