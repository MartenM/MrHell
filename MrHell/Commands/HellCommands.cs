using MrHell.Data.Models;
using MrHell.Players;
using PixelPilot.ChatCommands;
using PixelPilot.PixelGameClient;
using PixelPilot.PixelGameClient.Players;

namespace MrHell.Commands;

public class HellCommands : PixelChatCommandManager<HellPlayer>
{
    private PixelPilotClient _client;
    
    public HellCommands(PixelPilotClient client, PixelPlayerManager<HellPlayer> pixelPlayerManager) : base(client, pixelPlayerManager)
    {
        _client = client;
    }

    protected override ICommandSender CreateSender(HellPlayer player)
    {
        return new HellSender(player, _client);
    }
}

/// <summary>
/// Custom sender that does permission checks. Currently just one group that can do
/// permissions that require permission :)
/// </summary>
class HellSender : ICommandSender
{
    private PixelPilotClient _client;
    private HellPlayer _player;

    public HellSender(HellPlayer player, PixelPilotClient client)
    {
        _client = client;
        _player = player;
    }

    public void SendMessage(string msg)
    {
        _client.SendPm(_player.Username, msg);
    }

    public bool HasPermission(string? permission)
    {
        // Always false on load.
        if (_player.Profile == null) return false;
        
        if (permission == null && _player.Role == PlayerRole.Default) return true;
        if (_player.Role == PlayerRole.Admin) return true;

        return false;
    }

    public IPixelPlayer Player => _player;
}