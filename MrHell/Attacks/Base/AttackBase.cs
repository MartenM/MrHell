using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks.Placed;

namespace MrHell.Attacks.Base;

/// <summary>
/// Implements some useful logic for most attacks.
/// Skips the first tick such that attacks always start where you expect them to be.
/// </summary>
public abstract class AttackBase : IAttack
{
    private bool _hasTicked = false;
    protected int TotalTicks = 0;
    public bool Tick(PixelWorld world)
    {
        if (!_hasTicked)
        {
            _hasTicked = true;
            return true;
        }

        TotalTicks++;
        return InternalTick(world);
    }

    protected abstract bool InternalTick(PixelWorld world);

    public abstract List<IPlacedBlock> GetBlocks(PixelWorld world);

    public virtual List<IAttack>? GetAttacks(PixelWorld world)
    {
        return null;
    }

    public virtual bool IsDestructive { get; }
}