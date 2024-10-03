using MrHell.Attacks.Base;
using MrHell.Util;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.SingleAttacks;

public class SafeSpaceAttack: IAttack
{
    private static Random _random = new();
    
    private int _safeSpot;
    private int _countDown = 10;
    private int _safeWidth;
    
    // Safespot calculation
    private int SafeSpotStart => _safeSpot - _safeWidth / 2;
    private int SafeSpotEnd => SafeSpotStart + _safeWidth;

    public SafeSpaceAttack(int safeSpot, int safeWidth)
    {
        _safeWidth = safeWidth;
        _safeSpot = safeSpot;
    }

    public bool Tick(PixelWorld world)
    {
        _countDown--;

        if (_countDown <= -5) return false;
        return true;
    }

    public List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        List<IPlacedBlock> blocks = new();
        if (_countDown < 0)
        {
            for (int i = 0; i < Platform.PlatformLength; i++)
            {
                if (Platform.StartX + i >= SafeSpotStart && Platform.StartX + i < SafeSpotEnd) continue;
                if (world.BlockAt(WorldLayer.Foreground, Platform.StartX + i, Platform.Y).Block == PixelBlock.Empty) continue;
                
                blocks.Add(new PlacedBlock(Platform.StartX + i, Platform.Y - 1, WorldLayer.Foreground, new BasicBlock(PixelBlock.HazardFire)));
            }
        }

        PixelBlock block = _countDown % 2 == 0 ? PixelBlock.PastelLimeBg : PixelBlock.NormalGreenBg;
        for (int i = 0; i < _safeWidth; i++)
        {
            if (world.BlockAt(WorldLayer.Foreground, SafeSpotStart + i, Platform.Y).Block == PixelBlock.Empty) continue;
            blocks.Add(new PlacedBlock(SafeSpotStart + i, Platform.Y - 1, WorldLayer.Background, new BasicBlock(block)));
        }

        return blocks;
    }

    public static SafeSpaceAttack FindSpace(PixelWorld world, int size)
    {
        int x = 0;
        do
        {
            x = Platform.StartX + _random.Next(Platform.PlatformLength - 4) + 2;
        } while (world.BlockAt(WorldLayer.Foreground, x, Platform.Y).Block == PixelBlock.Empty);
        
        return new SafeSpaceAttack(x, size);
    }
}