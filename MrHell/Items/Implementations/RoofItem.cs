using MrHell.Items.Base;
using MrHell.Players;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Implementations;

public class RoofItem : HellItem
{
    public RoofItem() : base("item.roof", "Roof", "Spawn a roof above your head!")
    {
    }

    public override Task Execute(HellPlayer player, IHellApi api)
    {
        var baseBlock = new BasicBlock(PixelBlock.IndustrialScaffoldingHorizontal);
        api.PlaceBlock(new PlacedBlock((int) player.X / 16 - 1, (int) player.Y / 16 - 3, WorldLayer.Foreground, baseBlock));
        api.PlaceBlock(new PlacedBlock((int) player.X / 16    , (int) player.Y / 16 - 3, WorldLayer.Foreground, baseBlock));
        api.PlaceBlock(new PlacedBlock((int) player.X / 16 + 1, (int) player.Y / 16 - 3, WorldLayer.Foreground, baseBlock));

        return Task.CompletedTask;
    }
}