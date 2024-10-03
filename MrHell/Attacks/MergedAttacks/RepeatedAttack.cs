using MrHell.Attacks.Base;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks.Placed;

namespace MrHell.Attacks.MergedAttacks;

public abstract class RepeatedAttack : AttackBase
{
    private int _totalTimes;
    private int _spawnsPerTime;
    private int _ticksBetween;

    public RepeatedAttack(int totalTimes, int spawnsPerTime, int ticksBetween)
    {
        _totalTimes = totalTimes;
        _spawnsPerTime = spawnsPerTime;
        _ticksBetween = ticksBetween;
    }

    protected override bool InternalTick(PixelWorld world)
    {
        return _totalTimes >= 1;
    }

    public override List<IPlacedBlock> GetBlocks(PixelWorld world)
    {
        return new(0);
    }

    public override List<IAttack>? GetAttacks(PixelWorld world)
    {
        if (TotalTicks % _ticksBetween == 0)
        {
            _totalTimes--;
            return Enumerable.Range(0, _spawnsPerTime).Select(_ => GetSpawn()).ToList();
        }

        return null;
    }

    public abstract IAttack GetSpawn();
}