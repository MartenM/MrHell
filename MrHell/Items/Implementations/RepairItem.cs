using MrHell.Items.Base;
using MrHell.Players;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Implementations;

public class RepairItem : HellItem
{
    public RepairItem() : base("item.repair", "Repair", "Repair the platform.")
    {
        
    }

    public override Task Execute(HellPlayer player, IHellApi api)
    {
        api.BuildPlatform(new BasicBlock(PixelBlock.BrickRed));
        return Task.CompletedTask;
    }
}