using MrHell.Attacks.Base;
using MrHell.Attacks.SingleAttacks;
using MrHell.Util;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.MergedAttacks;

public class FireRainAttack : RepeatedAttack
{
    public override IAttack GetSpawn()
    {
        return new FallingBlock(HellRandom.Next(Arena.Width) + Arena.StartX, 19,
            new BasicBlock(PixelBlock.HazardFire), false);
    }

    public FireRainAttack() : base(20, 2, 1)
    {
        
    }
}