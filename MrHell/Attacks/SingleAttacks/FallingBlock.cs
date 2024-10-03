using MrHell.Attacks.Base;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.SingleAttacks;

public class FallingBlock : AttackBase
{
    public override bool IsDestructive { get; }

    private int _x;
    private int _y;
    private IPixelBlock _block;

    public FallingBlock(int x, int y, IPixelBlock block, bool destructive = false)
    {
        _x = x;
        _y = y;
        _block = block;
        IsDestructive = destructive;
    }

    protected override bool InternalTick(PixelWorld world)
    {

        _y++;
        
        if (!Arena.InArena(_x, _y)) return false;
        
        return IsDestructive || world.BlockAt(WorldLayer.Foreground, _x, _y).Block == PixelBlock.Empty;
    }

    public override List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        return new List<IPlacedBlock>
        {
            new PlacedBlock(_x, _y, WorldLayer.Foreground, _block),
        };
    }
    
    
}