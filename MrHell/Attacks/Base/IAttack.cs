using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks.Placed;

namespace MrHell.Attacks.Base;

public interface IAttack
{
    /// <summary>
    /// Ticks the attack to move.
    /// </summary>
    /// <returns>If the move is still in action</returns>
    public bool Tick(PixelWorld world);
    
    /// <summary>
    /// Get the blocks placed by this attack for this tick.
    /// </summary>
    /// <returns>A list of blocks</returns>
    public List<IPlacedBlock> GetBlocks(PixelWorld world);

    /// <summary>
    /// Gets the attacks placed by this attack this tick.
    /// </summary>
    /// <returns></returns>
    public List<IAttack>? GetAttacks(PixelWorld world)
    {
        return null;
    }

    /// <summary>
    /// If set to true the blocks placed by the attack will destroy blocks in the arena.
    /// </summary>
    public bool IsDestructive => false;
}