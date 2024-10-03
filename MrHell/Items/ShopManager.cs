using System.Drawing;
using MrHell.Items.Base;
using MrHell.Players;
using MrHell.Util;
using PixelPilot.PixelGameClient;
using PixelPilot.PixelGameClient.Messages;
using PixelPilot.PixelGameClient.Messages.Received;
using PixelPilot.PixelGameClient.World;
using PixelPilot.PixelGameClient.World.Blocks.Effects;
using PixelPilot.PixelGameClient.World.Constants;

namespace MrHell.Items;

/// <summary>
/// Manages the shops found in a world.
/// Ensures they can be repopulated with offers.
/// </summary>
public class ShopManager
{
    private PixelWorld _world;
    private PixelPilotClient _client;
    private HellPlayerManager _playerManager;

    private List<ShopSign> _shopSigns = new();
    private List<ShopOffer> _shopOffers = new();
    
    public ShopManager(PixelPilotClient client, PixelWorld world, HellPlayerManager playerManager)
    {
        _world = world;
        _playerManager = playerManager;
        _client = client;
    }

    public void Init()
    {
        for (int x = 0; x < _world.Width; x++)
        {
            for (int y = 0; y < _world.Height; y++)
            {
                var block = _world.BlockAt(WorldLayer.Foreground, x, y);
                if (block.Block != PixelBlock.BeveledYellow) continue;
                
                _shopSigns.Add(new ShopSign(new Point(x, y - 1)));
            }
        }
        
        CreateOffers();
        RepopulateShopSigns();
    }

    public void HandlePacket(object sender, IPixelGamePacket packet)
    {
        var movePacket = packet as PlayerMovePacket;
        if (movePacket == null) return;

        if (movePacket.VelocityX != 0 || movePacket.VelocityY != 0) return;
        if (movePacket.Vertical != 1 || movePacket.Horizontal != 0) return;

        var player = _playerManager.GetPlayer(movePacket.PlayerId);
        if (player == null) return;
        if (player.Profile == null) return;
        
        // Check if position matches any shop.
        var playerX = (int) ((player.X) / 16 + 0.5);
        var playerY = (int) player.Y / 16;
            
        var selectedShop = _shopSigns
            .FirstOrDefault(s => s.Location.X == playerX && s.Location.Y == playerY);

        if (selectedShop == null) return;
        
        // Check if shop has an offer.
        if (selectedShop.Offer == null)
        {
            _client.SendPm(player.Username, "No offer is available for this shop.");
            return;
        }
        
        // Check if enough coins
        var offer = selectedShop.Offer;
        if (offer.Coins > player.Profile.Coins)
        {
            _client.SendPm(player.Username, $"Not enough coins to buy {offer.Item.Name}");
            return;
        }
        
        // Enough coins, buy the item.
        player.Profile.Coins -= offer.Coins;
        player.Profile.Inventory.Add(offer.Item.Id);
        _client.SendPm(player.Username, $"Bought {offer.Item.Name} (-{offer.Coins} coins)");
    }

    public void CreateOffers()
    {
        _shopOffers.Clear();
        _shopOffers.Add(new ShopOffer(
                ItemManager.Instance.GetItem("item.bomb"),
                PixelBlock.GenericStripedHazardBlack,
                10
            ));
        
        _shopOffers.Add(new ShopOffer(
            ItemManager.Instance.GetItem("item.repair"),
            PixelBlock.ConstructionConeOrange,
            5
        ));
        
        _shopOffers.Add(new ShopOffer(
            ItemManager.Instance.GetItem("item.roof"),
            PixelBlock.IndustrialScaffoldingHorizontal,
            8
        ));
        
        _shopOffers.Add(new ShopOffer(
            ItemManager.Instance.GetItem("item.boosts"),
            PixelBlock.BoostUp,
            5
        ));
        
        _shopOffers.Add(new ShopOffer(
            ItemManager.Instance.GetItem("item.void"),
            PixelBlock.ScifiPanelMagenta,
            5
        ));
        
        _shopOffers.Add(new ShopOffer(
            ItemManager.Instance.GetItem("item.curse"),
            new TimedEffectBlock(PixelBlock.EffectsCurse, 1),
            10
        ));
    }

    public void RepopulateShopSigns()
    {
        var offers = _shopOffers.ToList();

        // Give the shops new offers.
        foreach (var shop in _shopSigns)
        {
            if (offers.Count == 0)
            {
                shop.Offer = null;
                continue;
            }
            
            var offer = offers[HellRandom.Next(offers.Count)];
            offers.Remove(offer);

            shop.Offer = offer;
        }

        // Update the shops.
        foreach (var shop in _shopSigns)
        {
            foreach (var packet in shop.ShopBlocks().Select(b => b.AsPacketOut()))
            {
                _client.Send(packet);
            }
        }
    }
}