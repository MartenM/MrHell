using MrHell.Attacks.Base;
using MrHell.Attacks.SingleAttacks;
using MrHell.Util;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Attacks.MergedAttacks;

public class MeteorShower : RepeatedAttack
{
    public MeteorShower() : base(7, 1, 4)
    {
    }

    public override IAttack GetSpawn()
    {
        return new MeteorAttack(HellRandom.Next(Platform.PlatformLength) + Platform.StartX, 19,
            new BasicBlock(PixelBlock.LavaOrange));
    }
}