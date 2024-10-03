using MrHell.Attacks.Base;
using MrHell.Items.Base;
using MrHell.Players;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items.Implementations;

public class BombItem : HellItem
{
    public BombItem() : base("item.bomb", "Bomb", "Place a bomb near your feet.")
    {
    }

    public override Task Execute(HellPlayer player, IHellApi api)
    {
        api.SpawnAttack(new BombAttack((int) player.X / 16, ((int) player.Y / 16) + 1));
        return Task.CompletedTask;
    }

    private class BombAttack : AttackBase
    {
        private int _x;
        private int _y;
        private int _countDown = 5;

        public override bool IsDestructive => true;

        public BombAttack(int x, int y)
        {
            _x = x;
            _y = y;
        }
        protected override bool InternalTick(PixelWorld world)
        {

            if (_countDown <= -10) return false;
            _countDown--;
            return true;
        }

        public override List<IPlacedBlock> GetBlocks(PixelWorld world)
        {
            if (_countDown >= 0)
            {
                var block = _countDown % 2 == 0
                    ? PixelBlock.GenericStripedHazardBlack
                    : PixelBlock.GenericStripedHazardYellow;

                return new List<IPlacedBlock>()
                {
                    new PlacedBlock(_x, _y, WorldLayer.Foreground, new BasicBlock(block))
                };
            }

            var blocks = new List<IPlacedBlock>();
            for (int i = -1; i < 2; i++)
            {
                blocks.Add(new PlacedBlock(_x + i, _y - 1, WorldLayer.Foreground, new BasicBlock(PixelBlock.HazardFire)));
                blocks.Add(new PlacedBlock(_x + i, _y, WorldLayer.Foreground, new BasicBlock(PixelBlock.HazardFire)));
                blocks.Add(new PlacedBlock(_x + i, _y + 1, WorldLayer.Foreground, new BasicBlock(PixelBlock.HazardFire)));
            }

            return blocks;
        }
    }
}