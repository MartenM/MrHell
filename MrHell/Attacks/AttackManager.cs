using System.Drawing;
using MrHell.Attacks.Base;
using MrHell.Util;
using PixelPilot.PixelGameClient;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;
using PixelPilot.Structures;
using PixelPilot.Structures.Extensions;

namespace MrHell.Attacks;

/// <summary>
/// Manages attacks.
/// Uses frames that are constructed each time. This means that in theory you could use commands like 'nextFrame' to see
/// animations play frame by frame.
/// </summary>
public class AttackManager
{
    private TaskCompletionSource<bool> _onEmptyTaskCompletionSource = new(); 
    
    private PixelPilotClient _client;
    private PixelWorld _world;

    private CancellationTokenSource _cancellationTokenSource = new();
    private Task? _task;

    private List<IAttack> _attacks = new();
    private List<IPlacedBlock> _previousPlaced = new();
    public int TicksPerSecond = 5;

    private Structure _original = null!;
    
    public AttackManager(PixelPilotClient client, PixelWorld world)
    {
        _client = client;
        _world = world;
    }

    public void Init()
    {
        _original = _world.GetStructure(new Point(Arena.StartX, Arena.StartY), new Point(Arena.EndX, Arena.EndY));
    }

    public void Start()
    {
        // Prevent running twice.
        _cancellationTokenSource.Cancel();
        _task?.Wait();

        _cancellationTokenSource = new CancellationTokenSource();
        _task = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                _applyTick();
                await Task.Delay(1000 / TicksPerSecond);
            }
        });
    }

    private void _applyTick()
    {
        // Get all blocks that need to be placed.
        List<IPlacedBlock> blocks = new();
            
        // Lock this since attacks might be added at the same time!
        lock (_attacks)
        {
            // Tick all attacks. Remove the ones that are no longer valid.
            _attacks.RemoveAll(w => !w.Tick(_world));
            
            // Get blocks that should be displayed now.
            foreach (var attack in _attacks)
            {
                if (attack.IsDestructive)
                {
                    blocks.AddRange(attack.GetBlocks(_world));
                    continue; 
                }
                
                // Attack is not destructive. Check the world for existing blocks.
                // That are NOT part of an attack.
                foreach (var block in attack.GetBlocks(_world))
                {
                    // We don't care about backgrounds, least for now.
                    if ((WorldLayer) block.Layer == WorldLayer.Background)
                    {
                        blocks.Add(block);
                        continue;
                    }
                    
                    if (_previousPlaced.Any(b => b.Layer == block.Layer && b.X == block.X && b.Y == block.Y) || _world.BlockAt(block.Layer, block.X, block.Y).Block == PixelBlock.Empty)
                    {
                        blocks.Add(block);
                        continue;
                    }
                    
                    // Would destory a block, don't add.
                }
            }
            
            // Add attacks that are spawned this iteration.
            List<IAttack> addedAttacks = new List<IAttack>(1);
            foreach (var attack in _attacks)
            {
                var addition = attack.GetAttacks(_world);
                if (addition == null) continue;
                addedAttacks.AddRange(addition);
            }
            
            // Tick added attacks once.
            addedAttacks.RemoveAll(w => !w.Tick(_world));
            _attacks.AddRange(addedAttacks);
        }
        
        // Remove any attacks that go out of bounds.
        blocks.RemoveAll(b => !Arena.InArena(b.X, b.Y));
        
        var nextPrevious = new List<IPlacedBlock>(blocks);
            
        // All previous blocks that don't fill a spot anymore should be removed to air.
        foreach (var prevBlock in _previousPlaced)
        {
            if (blocks.Any(b => b.Layer == prevBlock.Layer && b.X == prevBlock.X && b.Y == prevBlock.Y)) continue;

            if ((WorldLayer) prevBlock.Layer == WorldLayer.Background)
            {
                // Get from struct such that we can reconstruct properly.
                var block = _original.Blocks
                    .Where(b => (WorldLayer) b.Layer == WorldLayer.Background && b.X == prevBlock.X - Arena.StartX && b.Y == prevBlock.Y - Arena.StartY)
                    .Select(b => b.Block).FirstOrDefault();

                if (block == null)
                {
                    // Unknown!
                    block = new BasicBlock(PixelBlock.BasicRedBg);
                }
                
                blocks.Add(new PlacedBlock(prevBlock.X, prevBlock.Y, prevBlock.Layer, block));
            }
            else
            {
                blocks.Add(new PlacedBlock(prevBlock.X, prevBlock.Y, prevBlock.Layer, new BasicBlock(PixelBlock.Empty)));
            }
        }
            
        // Set previous to now.
        _previousPlaced = nextPrevious;
            
        // Chunk them together.
        var chunks = blocks.ToChunkedPackets();
        foreach (var packet in chunks)
        {
            _client.Send(packet);
        }

        lock (_attacks)
        {
            if (_attacks.Count == 0 && chunks.Count == 0)
            {
                _onEmptyTaskCompletionSource.TrySetResult(true);
            }
        }
    }

    public async Task Stop()
    {
        _cancellationTokenSource.Cancel();
        if (_task != null)
        {
            await _task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        
        lock (_attacks)
        {
            _attacks.Clear();
        }
    }

    public void AddAttack(IAttack attack)
    {
        lock (_attacks)
        {
            _attacks.Add(attack);
        }
    }

    public Task WaitForEmpty()
    {
        lock (_attacks)
        {
            if (_attacks.Count == 0) return Task.CompletedTask;
        }

        return _onEmptyTaskCompletionSource.Task;
    }

    public void ResetEmptyWait()
    {
        _onEmptyTaskCompletionSource = new();
    }
}