using MrHell.Attacks.Base;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.SingleAttacks;

public class HorizontalMovingBlockAttack : AttackBase
{
    public enum Direction
    {
        Right,
        Left,
    }

    private int _x;
    private int _y;
    private int _height;
    private Direction _direction;
    private IPixelBlock _block;

    public HorizontalMovingBlockAttack(int x, int y, int height, Direction direction, IPixelBlock block)
    {
        _x = x;
        _y = y;
        _height = height;
        _direction = direction;
        _block = block;
    }

    protected override bool InternalTick(PixelWorld world)
    {
        switch (_direction)
        {
            case Direction.Right:
                _x++;
                break;
            case Direction.Left:
                _x--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return Arena.InArena(_x, _y);
    }

    public override List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        List<IPlacedBlock> blocks = new(_height);
        for (int i = 0; i < _height; i++)
        {
            blocks.Add(new PlacedBlock(_x, _y - i, WorldLayer.Foreground, _block));
        }

        return blocks;
    }
}