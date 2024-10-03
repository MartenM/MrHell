using MrHell.Attacks.Base;
using MrHell.Attacks.SingleAttacks;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.MergedAttacks;

public class RepeatedWall : AttackBase
{
    private int _totalWalls;
    private IPixelBlock _block;
    private int _ticksBetween = 10;

    private HorizontalMovingBlockAttack.Direction _direction;

    public RepeatedWall(int totalWalls, HorizontalMovingBlockAttack.Direction direction, IPixelBlock block)
    {
        _totalWalls = totalWalls;
        _block = block;
        _direction = direction;
    }

    protected override bool InternalTick(PixelWorld world)
    {
        return _totalWalls >= 1;
    }

    public override List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        return new();
    }

    public override List<IAttack>? GetAttacks(PixelWorld world)
    {
        if (TotalTicks % _ticksBetween == 0)
        {
            _totalWalls--;

            int x;
            int y = Platform.Y - 1;
            if (_direction == HorizontalMovingBlockAttack.Direction.Right)
            {
                x = 14;
            }
            else
            {
                x = 35;
            }

            return new List<IAttack>()
            {
                new HorizontalMovingBlockAttack(x, y, HellRandom.Next(4), _direction, _block)
            };
        }

        return null;
    }

    public static RepeatedWall Arrow(int totalWalls)
    {
        var direction = (HellRandom.Next(2) == 0)
            ? HorizontalMovingBlockAttack.Direction.Left
            : HorizontalMovingBlockAttack.Direction.Right;

        var block = direction == HorizontalMovingBlockAttack.Direction.Left
            ? PixelBlock.GravityLeft
            : PixelBlock.GravityRight;

        return new RepeatedWall(totalWalls, direction, new BasicBlock(block));
    }
    
    public static RepeatedWall Booster(int totalWalls)
    {
        var direction = (HellRandom.Next(2) == 0)
            ? HorizontalMovingBlockAttack.Direction.Left
            : HorizontalMovingBlockAttack.Direction.Right;

        var block = direction == HorizontalMovingBlockAttack.Direction.Left
            ? PixelBlock.BoostLeft
            : PixelBlock.BoostRight;

        return new RepeatedWall(totalWalls, direction, new BasicBlock(block));
    }
    
    public static RepeatedWall Fire(int totalWalls)
    {
        var direction = (HellRandom.Next(2) == 0)
            ? HorizontalMovingBlockAttack.Direction.Left
            : HorizontalMovingBlockAttack.Direction.Right;

        return new RepeatedWall(totalWalls, direction, new BasicBlock(PixelBlock.HazardFire));
    }
}