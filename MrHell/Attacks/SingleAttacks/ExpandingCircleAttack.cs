using MrHell.Attacks.Base;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.SingleAttacks;

public class ExpandingCircleAttack : IAttack
{
    private int _centerX;
    private int _centerY;
    private int _currentRadius;
    private int _maxRadius;
    private int _countDown;
    private BasicBlock _block;
    
    // Circle expands every X ticks (delayed effect)
    private const int TicksPerExpansion = 3;

    public ExpandingCircleAttack(int centerX, int centerY, int maxRadius, BasicBlock block)
    {
        _centerX = centerX;
        _centerY = centerY;
        if (_centerY == Platform.Y)
        {
            _centerY -= 1;
        }
        
        _maxRadius = maxRadius;
        _block = block;
        _currentRadius = 0;
        _countDown = 5; 
    }

    public bool Tick(PixelWorld world)
    {
        _countDown--;

        // Expand the circle every few ticks
        if (_countDown <= 0)
        {
            _currentRadius++;
            _countDown = TicksPerExpansion; // Reset the countdown for next expansion
        }

        // Stop if the circle reaches the maximum radius
        if (_currentRadius > _maxRadius) return false;

        return true;
    }

    public List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        List<IPlacedBlock> blocks = new();

        blocks.Add(new PlacedBlock(_centerX - 1, _centerY, WorldLayer.Background, new BasicBlock(PixelBlock.EnvironmentLavaBg)));
        blocks.Add(new PlacedBlock(_centerX + 1, _centerY, WorldLayer.Background, new BasicBlock(PixelBlock.EnvironmentLavaBg)));
        blocks.Add(new PlacedBlock(_centerX, _centerY - 1, WorldLayer.Background, new BasicBlock(PixelBlock.EnvironmentLavaBg)));
        blocks.Add(new PlacedBlock(_centerX, _centerY + 1, WorldLayer.Background, new BasicBlock(PixelBlock.EnvironmentLavaBg)));
        blocks.Add(new PlacedBlock(_centerX, _centerY, WorldLayer.Background, new BasicBlock(PixelBlock.EnvironmentLavaBg)));
        
        if (_currentRadius == 0) return blocks;
        
        blocks.Add(new PlacedBlock(_centerX, _centerY, WorldLayer.Foreground, new BasicBlock(PixelBlock.EnvironmentLava)));
        
        // Only add blocks on the edge of the current radius
        if (_currentRadius <= _maxRadius)
        {
            // Calculate the blocks around the circumference of the circle
            for (int angle = 0; angle < 360; angle += 10) // Step by 10 degrees for simplicity
            {
                // Convert polar coordinates (angle, radius) to Cartesian coordinates (x, y)
                int x = _centerX + (int)(_currentRadius * Math.Cos(Math.PI * angle / 180));
                int y = _centerY + (int)(_currentRadius * Math.Sin(Math.PI * angle / 180));

                // Ensure blocks stay within arena bounds
                if (Arena.InArena(x, y))
                {
                    blocks.Add(new PlacedBlock(x, y, WorldLayer.Foreground, _block));
                }
            }
        }
        
        if (_currentRadius - 2 > 0)
        {
            // Calculate the blocks around the circumference of the circle
            for (int angle = 0; angle < 360; angle += 10) // Step by 10 degrees for simplicity
            {
                // Convert polar coordinates (angle, radius) to Cartesian coordinates (x, y)
                int x = _centerX + (int)((_currentRadius - 2) * Math.Cos(Math.PI * angle / 180));
                int y = _centerY + (int)((_currentRadius - 2) * Math.Sin(Math.PI * angle / 180));

                // Ensure blocks stay within arena bounds
                if (Arena.InArena(x, y))
                {
                    blocks.Add(new PlacedBlock(x, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.LavaYellow)));
                }
            }
        }

        return blocks;
    }
}