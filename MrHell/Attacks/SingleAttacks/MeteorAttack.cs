using MrHell.Attacks.Base;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.SingleAttacks;

public class MeteorAttack : AttackBase
{
    private int _x;
    private int _y;

    private bool _exploded = false;

    private IPixelBlock _fallingBlock;
    private IPixelBlock _destructionBlock = new BasicBlock(PixelBlock.HazardFire);

    public MeteorAttack(int x, int y, IPixelBlock fallingBlock)
    {
        _x = x;
        _y = y;
        _fallingBlock = fallingBlock;
    }

    public override bool IsDestructive => false;

    protected override bool InternalTick(PixelWorld world)
    {
        if (!Arena.InArena(_x, _y)) return false;

        if (_exploded) return TotalTicks < 2;
        
        if (world.BlockAt(WorldLayer.Foreground, _x, _y + 1).Block != PixelBlock.Empty)
        {
            _exploded = true;
            TotalTicks = 0;
            return true;
        }

        if (TotalTicks % 2 == 0)  _y++;

        return true;
    }

    public override List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        var blocks = new List<IPlacedBlock>(9);
        if (_exploded)
        {
            for (int i = 0; i < 3; i++)
            {
                blocks.Add(new PlacedBlock(_x + i - 1, _y, WorldLayer.Foreground, _destructionBlock));
            }

            return blocks;
        }

        return new List<IPlacedBlock>()
        {
            new PlacedBlock(_x, _y, WorldLayer.Foreground, _fallingBlock)
        };
    }
}