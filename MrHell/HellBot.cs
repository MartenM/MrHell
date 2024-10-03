using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrHell.Attacks;
using MrHell.Attacks.Base;
using MrHell.Attacks.MergedAttacks;
using MrHell.Attacks.SingleAttacks;
using MrHell.Commands;
using MrHell.Commands.AdminCommands;
using MrHell.Commands.GameCommands;
using MrHell.Configuration;
using MrHell.Data;
using MrHell.Items;
using MrHell.Players;
using MrHell.Util;
using PixelPilot.PixelGameClient;
using PixelPilot.PixelGameClient.Messages;
using PixelPilot.PixelGameClient.Messages.Received;
using PixelPilot.PixelGameClient.Messages.Send;
using PixelPilot.PixelGameClient.Players;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks;
using PixelPilot.PixelGameClient.World.Blocks.Placed;
using PixelPilot.PixelGameClient.World.Constants;
using PixelPilot.Structures.Extensions;

namespace MrHell;

/// <summary>
/// Base class for the bot.
/// TODO: Split the actual game from this.
/// </summary>
public class HellBot : IHellApi
{
    private static Random _random = new();
    
    private BotOptions _botOptions;
    private ILogger<HellBot> _logger;
    
    
    private PixelPilotClient _client = null!;
    private HellPlayerManager _playerManager = null!;
    private PixelWorld _world = null!;
    private HellCommands _hellCommands = null!;
    
    // Game logic
    private List<HellPlayer> _playing = new();

    private CancellationTokenSource _monitorCancellation = new();
    private Task? _monitorTask;

    private CancellationTokenSource _gameTaskCancellation = new();
    private Task? _gameTask;
    
    // Game
    private AttackManager _attackManager = null!;
    private bool _isRanked = true;
    public bool AutomaticStart { get; private set; } = false;
    public bool Endless { get; set; }
    public bool OngoingGame => _gameTask != null && !_gameTask.IsCompleted;
    
    // Misc
    private ShopManager _shopManager = null!;

    public HellBot(ILogger<HellBot> logger, IOptions<BotOptions> botOptions)
    {
        _logger = logger;
        _botOptions = botOptions.Value;
    }

    public async Task Start()
    {
        _logger.LogInformation("Starting hell...");
        
        _client = PixelPilotClient.Builder()
            .SetEmail(_botOptions.AccountEmail!)
            .SetPassword(_botOptions.AccountPassword!)
            .SetPrefix("[MrHell] ")
            .Build();

        ItemManager itemManager = new ItemManager();
        itemManager.RegisterAllItems();

        _playerManager = new HellPlayerManager();
        _client.OnPacketReceived += _playerManager.HandlePacket;

        _world = new PixelWorld();
        _client.OnPacketReceived += _world.HandlePacket;

        _shopManager = new ShopManager(_client, _world, _playerManager);
        _client.OnPacketReceived += _shopManager.HandlePacket;

        _hellCommands = new HellCommands(_client, _playerManager);

        _hellCommands.AddHelpCommand();
        
        _hellCommands.AddCommand(new WinsCommand());
        _hellCommands.AddCommand(new CoinsCommand(_playerManager));
        _hellCommands.AddCommand(new AfkCommand());
        
        _hellCommands.AddCommand(new InventoryCommand());
        _hellCommands.AddCommand(new SelectItemCommand());
        
        _hellCommands.AddCommand(new EndlessCommand(this));
        _hellCommands.AddCommand(new StartCommand(this));
        _hellCommands.AddCommand(new CancelCommand(this));
        _hellCommands.AddCommand(new AutoCommand(this));
        _hellCommands.AddCommand(new DisconnectCommand(this));
        _hellCommands.AddCommand(new PresentCommand(_playerManager, _client));
        _hellCommands.AddCommand(new ShopRefreshCommand(_shopManager));
        
        // Misc
        _hellCommands.AddCommand(new SpawnCoinsCommand());
        
        _client.OnPacketReceived += HandlePacket;
        
        _attackManager = new AttackManager(_client, _world);

        await _client.Connect(_botOptions.WorldId!);
        await _world.InitTask;

        await _resetAct();
        _client.SendChat("I have awoken... Fear me...");
        
        // Init some stuff
        // Do things that require the world.
        _attackManager.Init();
        _shopManager.Init();
    }

    public void HandlePacket(object sender, IPixelGamePacket packet)
    {
        var playerPacket = packet as IPixelGamePlayerPacket;
        if (playerPacket == null) return;

        var player = _playerManager.GetPlayer(playerPacket.PlayerId);
        if (player == null) return;

        switch (playerPacket)
        {
            case PlayerRespawnPacket:
            case PlayerResetPacket:
                // Reward all players with a coin.
                _playing.Remove(player);
                _playing.ForEach(p =>
                {
                    if (p.Profile != null) p.Profile.Coins += 1;
                });
                break;
            case PlayerGodmodePacket:
            case PlayerModMode:
                _playing.Remove(player);
                break;
            case PlayerMovePacket move:
                player.LastMovement = DateTime.UtcNow;
                if (player.IsForcedAfk)
                {
                    player.IsForcedAfk = false;
                    _client.SendPm(player.Username, "You are no longer marked as AFK.");
                }
                
                _handleItemUse(player, move);
                break;
        }
    }

    public void _handleItemUse(HellPlayer player, PlayerMovePacket move)
    {
        // Check if it's a clea downwards press.
        if (move.Vertical == 1)
        {
            player.CleanDown++;
        }

        if (move.Horizontal != 0) player.CleanDown = 0;

        if (player.CleanDown < 3)
        {
            return;
        }

        // Check the following:
        // * Is playing
        // * Has item selected
        // * In the arena
        // * Cooldown of 2 seconds
        if (!_playing.Contains(player) || player.SelectedItem == null ||
            (DateTime.UtcNow - player.LastItemUse) <= TimeSpan.FromSeconds(2) ||
            !Arena.InArena((int)player.X / 16, (int)player.Y / 16)) return;
        
        // Use the item!
        var item = player.SelectedItem;
        player.Profile?.Inventory.Remove(item.Id);
        if (!player.Profile?.Inventory.Contains(item.Id) ?? false)
        {
            player.SelectedItem = null;
        }
                        
        _client.SendPm(player.Username, $"Used item: {item.Name}");
        player.LastItemUse = DateTime.UtcNow;
        _ = item.Execute(player, this);
    }

    public async Task Run(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting game loop task...");
        
        // Start monitor task
        _monitorTask = _startMonitor();
        
        // Either wait for stop, or disconnect.
        await _client.WaitForDisconnect(stoppingToken);
    }

    /// <summary>
    /// Monitor task, manages starting of the game when required.
    /// </summary>
    private async Task _startMonitor()
    {
        // Cancel original and start again.
        await _monitorCancellation.CancelAsync();
        _monitorCancellation = new CancellationTokenSource();

        // Main game loop
        int idleTimes = 0;
        while (!_monitorCancellation.IsCancellationRequested)
        {
            if (_gameTask != null) await _gameTask;
            
            if (_gameTask != null || AutomaticStart == false || _playerManager.Players.Where(p => p.IsAvailable).Count() <= 1)
            {
                idleTimes = 0;
                await Task.Delay(TimeSpan.FromSeconds(1));
                continue;
            }

            idleTimes++;
            if (idleTimes <= 20)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                continue;
            }

            idleTimes = 0;

            _logger.LogInformation("Starting a new game.");
            await StartGameSafe();
            _logger.LogInformation("Game has finished.");
        }
        
        _logger.LogWarning("Monitor task has finished processing.");
    }

    /// <summary>
    /// Save game start. Wraps it using a try {} catch {} block.
    /// Logs any errors that can occur.
    /// </summary>
    public async Task StartGameSafe()
    {
        _gameTask = Task.Run(async () =>
        {
            try
            {
                await _startGameInternal();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured in the main game loop.");
            }
        });

        // Await the game.
        await _gameTask;
        _gameTask = null;
    }

    /// <summary>
    /// A game of MrHell.
    /// </summary>
    private async Task _startGameInternal()
    {
        await _resetAct();
        
        await _gameTaskCancellation.CancelAsync();
        _gameTaskCancellation = new CancellationTokenSource();

        _playing = _playerManager.Players.Where(p => p.IsAvailable).ToList();
        _isRanked = _playing.Count >= 3;
        
        _clearArena();
        _client.Send(new PlacedBlock(38, 41, WorldLayer.Foreground, new BasicBlock(PixelBlock.CoinBlue)).AsPacketOut());
        
        foreach (var player in _playing)
        {
            _teleport(player, 38, 40);
        }
        
        _buildPlatform(PixelBlock.BasicRed);

        await _client.WaitForEmptyQueue();
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        _client.Send(new PlacedBlock(38, 41, WorldLayer.Foreground, new BasicBlock(PixelBlock.Empty)).AsPacketOut());
        
        // Teleport all players
        foreach (var player in _playing)
        {
            int y = 26;
            int x = _random.Next(29 - 20) + 20;
            
            _teleport(player, x, y);
            player.Profile!.Coins += 2;
        }
        
        await _client.WaitForEmptyQueue();
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        _client.SendChat("Always good to see new challengers. Lets see if your sacrifice is going to be enough!");
        if (!_isRanked) _client.SendChat("UNRANKED - Wins require at least 3 participants!");
        
        _attackManager.Start();
        
        // Start actual game loop with different attacks whoah epic.
        int rounds = 0;
        _attackManager.TicksPerSecond = 5;
        while ((Endless && _playing.Count > 0) || _playing.Count > 1)
        {
            if (_gameTaskCancellation.IsCancellationRequested) return;
            
            
            // Choose an attack!
            var chance = _random.Next(100);
            if (rounds <= 1 || chance < 15)
            {
                if (rounds <= 1)
                {
                    _attackManager.AddAttack(RepeatedWall.Arrow(_random.Next(2) + 3));
                }
                else if (HellRandom.Bool())
                {
                    _attackManager.AddAttack(RepeatedWall.Booster(_random.Next(2) + 4));
                }
                else
                {
                    _attackManager.AddAttack(RepeatedWall.Fire(_random.Next(2) + 4));
                }
            } else if (chance < 25)
            {
                _attackManager.AddAttack(SafeSpaceAttack.FindSpace(_world, HellRandom.Next(3) + 1));
            } else if (chance < 35)
            {
                _attackManager.AddAttack(new FireRainAttack());
            } else if (chance < 45)
            {
                _attackManager.AddAttack(new MeteorShower());
            }
            else if (chance < 55)
            {
                _attackManager.AddAttack(new MeteorShower());
            }
            else if (chance < 65)
            {
                _attackManager.AddAttack(new CirclesAttack());
            } else if (chance < 75)
            {
                _attackManager.AddAttack(new DestructionMeteor(HellRandom.Next(Platform.PlatformLength) + Platform.StartX, 19, new BasicBlock(PixelBlock.GenericStripedHazardBlack), new BasicBlock(PixelBlock.ScifiLaserOrangeStraightHorizontal)));
                _attackManager.AddAttack(new DestructionMeteor(HellRandom.Next(Platform.PlatformLength) + Platform.StartX, 19, new BasicBlock(PixelBlock.GenericStripedHazardBlack), new BasicBlock(PixelBlock.ScifiLaserOrangeStraightHorizontal)));
            }
            else if (chance < 85)
            {
                _attackManager.AddAttack(new MeteorShower());
                _attackManager.AddAttack(RepeatedWall.Arrow(_random.Next(3) + 2));
            }
            else if (chance < 95)
            {
                _attackManager.AddAttack(RepeatedWall.Booster(_random.Next(3) + 3));
                await Task.Delay(1000);
                _attackManager.AddAttack(RepeatedWall.Arrow(_random.Next(3) + 3));
            }
            else
            {
                var amount = _random.Next(3) + 3;
                _attackManager.AddAttack(new RepeatedWall(amount, HorizontalMovingBlockAttack.Direction.Left, new BasicBlock(PixelBlock.HazardFire)));
                _attackManager.AddAttack(new RepeatedWall(amount, HorizontalMovingBlockAttack.Direction.Right, new BasicBlock(PixelBlock.HazardFire)));
            }
            
            _attackManager.ResetEmptyWait();
            await _attackManager.WaitForEmpty();
            
            
            await Task.Delay(250);
            
            // Check for players that have not moved 10 seconds long.
            // Remove these and mark them as AFK.
            Predicate<DateTime> playingAfkCheck = x => DateTime.UtcNow - x > TimeSpan.FromSeconds(20);
            var markedPlayers = _playerManager.Players.Where(p => playingAfkCheck(p.LastMovement)).ToList();
            
            // Remove all these players and mark them as forced AFK
            _playing.RemoveAll(p => playingAfkCheck(p.LastMovement));
            foreach (var player in markedPlayers)
            {
                _setForcedAFK(player);
            }
            
            // Increase rounds played.
            rounds++;
            _attackManager.TicksPerSecond = rounds / 5 + 5;
        }

        if (_playing.Count == 1)
        {
            // We have a winner
            var winner = _playing.First();

            if (_isRanked)
            {
                winner.Profile!.Wins += 1;
                winner.Profile!.Coins += 5;
            }

            await using (var context = new HellContext())
            {
                context.Profiles.Attach(winner.Profile!);
                await context.SaveChangesAsync();
            }

            await _actBotLose(winner.Username);
            
            _teleport(winner, 11, 40);
            await Task.Delay(TimeSpan.FromSeconds(3));
            _teleport(winner, 18, 42);
        }
        else
        {
            await _actBotWin();
        }

        // Refresh shop
        _shopManager.RepopulateShopSigns();
        
        await _attackManager.Stop();
    }

    private void _setForcedAFK(HellPlayer player)
    {
        player.IsForcedAfk = true;
        _teleport(player, 31, 42);
    }

    private void _teleport(IPixelPlayer player, int x, int y)
    {
        _client.SendChat($"/tp @a[id={player.Id}] {x} {y}", prefix: false);
    }


    private void _buildPlatform(IPixelBlock block)
    {
        var placements = new List<IPlacedBlock>();
        for (int x = 17; x <= 32; x++)
        {
            placements.Add(new PlacedBlock(x, 27, WorldLayer.Foreground, block));
        }

        _client.Send(placements.ToChunkedPacket());
    }
    
    private void _buildPlatform(PixelBlock block)
    {
        _buildPlatform(new BasicBlock(block));
    }

    private void _clearArena()
    {
        var blocks = new List<IPlacedBlock>((35 - 14) * (30 - 19));
        for (int x = 14; x <= 35; x++)
        {
            for (int y = 19; y <= 30; y++)
            {
                blocks.Add(new PlacedBlock(x, y, WorldLayer.Foreground, new BasicBlock(PixelBlock.Empty)));
            }
        }

        var packets = blocks.ToChunkedPackets();
        foreach (var packet in packets)
        {
            _client.Send(packet);
        }
    }

    private async Task _actBotLose(string username)
    {
        for (int i = 0; i < 4; i++)
        {
            _client.Send(new PlacedBlock(23 + i, 22, WorldLayer.Foreground, new BasicBlock(PixelBlock.CanvasRed)).AsPacketOut());
            await Task.Delay(100);
        }
        
        _client.Send(new PlayerMoveOutPacket(24.5 * 16, 16.5 * 16, 0, 1.5, 0, 0, 0, 0, false, false, false, MoveTicks.Next()));
        await Task.Delay(250);
        _client.Send(new PlayerGodmodeOutPacket(false));
        await Task.Delay(250);

        if (_random.Next(100) < 25)
        {
            _client.SendChat($"{username} has defeated me by sacrificing the other players!");
        }
        else
        {
            _client.SendChat($"I have been defeated by {username}!");
        }
        
        for (int i = 0; i < 4; i++)
        {
            _client.Send(new PlacedBlock(23 + i, 21, WorldLayer.Foreground, new BasicBlock(PixelBlock.HazardFire)).AsPacketOut());
            await Task.Delay(100);
        }
        
        for (int i = 0; i < 4; i++)
        {
            _client.Send(new PlacedBlock(23 + i, 21, WorldLayer.Foreground, new BasicBlock(PixelBlock.Empty)).AsPacketOut());
            await Task.Delay(100);
        }
    }

    private async Task _actBotWin()
    {
        for (int i = 0; i < 4; i++)
        {
            _client.Send(new PlacedBlock(23 + i, 22, WorldLayer.Foreground, new BasicBlock(PixelBlock.StoneGray)).AsPacketOut());
            await Task.Delay(100);
        }
        
        _client.Send(new PlacedBlock(23 , 21, WorldLayer.Foreground, new BasicBlock(PixelBlock.DungeonTorchYellow)).AsPacketOut());
        _client.Send(new PlacedBlock(26 , 21, WorldLayer.Foreground, new BasicBlock(PixelBlock.DungeonTorchYellow)).AsPacketOut());
        
        _client.Send(new PlayerMoveOutPacket(24.5 * 16, 16.5 * 16, 0, 1.5, 0, 0, 0, 0, false, false, false, MoveTicks.Next()));
        await Task.Delay(250);
        _client.Send(new PlayerGodmodeOutPacket(false));
        await Task.Delay(250);
        
        _client.SendChat("I win! Nobody survived!");
    }

    private Task _resetAct()
    {
        _client.Send(new PlayerGodmodeOutPacket(true));
        _client.Send(new PlayerMoveOutPacket(24.5 * 16, 16.5 * 16, 0, 0, 0, 0, 0, 0, false, false, false, MoveTicks.Next()));
        return Task.CompletedTask;
    }

    public async Task CancelGame()
    {
        if (_gameTask == null) return;
        await _gameTaskCancellation.CancelAsync();
        await _gameTask;
    }

    public void ToggleAutomatic()
    {
        AutomaticStart = !AutomaticStart;
        _client.SendChat(AutomaticStart ? "/title Mr. Hell [ON]" : "/title Mr. Hell [???]", prefix: false);
    }

    /// <summary>
    /// Stops the bot gracefully.
    /// </summary>
    public async Task Stop()
    {
        if (_client.IsConnected)
        {
            _client.SendChat("/title Mr. Hell [OFF]", prefix: false);
            await Task.Delay(250);
        }
        
        // Save players first, just in case something crashes!
        await _playerManager.SaveAll();

        await _attackManager.Stop();
        await _monitorCancellation.CancelAsync();
        await _gameTaskCancellation.CancelAsync();
        
        await _client.Disconnect();
        
        
    }

    public void PlaceBlocks(List<IPlacedBlock> blocks)
    {
        var packets = blocks
            .Where(b => Arena.InArena(b.X, b.Y))
            .Select(b => b.AsPacketOut());
        
        foreach (var packet in packets)
        {
            _client.Send(packet);
        }
    }

    public void PlaceBlock(IPlacedBlock block)
    {
        if (!Arena.InArena(block.X, block.Y)) return;
        _client.Send(block.AsPacketOut());
    }

    public void BuildPlatform(IPixelBlock block)
    {
        _buildPlatform(block);
    }

    public void SpawnAttack(IAttack attack)
    {
        _attackManager.AddAttack(attack);
    }
}