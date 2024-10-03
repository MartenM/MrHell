using MrHell.Attacks.Base;
using MrHell.Attacks.SingleAttacks;
using MrHell.Util;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.MergedAttacks;

public class CirclesAttack : RepeatedAttack
{
    public CirclesAttack() : base(3, 1, 8)
    {
    }

    public override IAttack GetSpawn()
    {
        return new ExpandingCircleAttack(HellRandom.Next(Arena.Width) + Arena.StartX, HellRandom.Next(Arena.Height) + Arena.StartY,
            HellRandom.Next(3) + 2, new BasicBlock(PixelBlock.HazardFire));
    }
}