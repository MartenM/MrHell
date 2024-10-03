using MrHell.Attacks.Base;
using MrHell.Items.Base;
using MrHell.Players;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Implementations;

public class VoidItem : HellItem
{
    public VoidItem() : base("item.void", "Void", "Removes the blocks under you!")
    {
        
    }

    public override Task Execute(HellPlayer player, IHellApi api)
    {
        api.SpawnAttack(new BoosterAttack((int) player.X / 16, Platform.Y));
        return Task.CompletedTask;
    }

    private class BoosterAttack : AttackBase
    {
        private int _x;
        private int _y;
        public override bool IsDestructive => true;
        public BoosterAttack(int x, int y)
        {
            _x = x;
            _y = y;
        }

        protected override bool InternalTick(PixelWorld world)
        {
            return TotalTicks <= 15;
        }

        public override List<IPlacedBlock> GetBlocks(PixelWorld world)
        {
            BasicBlock block;
            if (TotalTicks < 10)
            {
                block = TotalTicks % 2 == 0
                    ? new BasicBlock(PixelBlock.GemstonePurple)
                    : new BasicBlock(PixelBlock.ScifiPanelMagenta);
            }
            else
            {
                block = new BasicBlock(PixelBlock.Empty);
            }
            
            return new List<IPlacedBlock>()
            {
                new PlacedBlock(_x - 1, _y, WorldLayer.Foreground, block),
                new PlacedBlock(_x, _y, WorldLayer.Foreground, block),
                new PlacedBlock(_x + 1, _y, WorldLayer.Foreground, block)
            };
        }
    }
}