using MrHell.Attacks.Base;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;

namespace MrHell;

/// <summary>
/// Used to pass around where the functionality can be implemented depending on the requirements.
/// An item might require different block placement etc.
/// </summary>
public interface IHellApi
{
    public void PlaceBlocks(List<IPlacedBlock> blocks);

    public void PlaceBlock(IPlacedBlock block);
    public void BuildPlatform(IPixelBlock block);

    public void SpawnAttack(IAttack attack);
}