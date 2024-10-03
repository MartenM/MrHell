using System.Reflection;
using Microsoft.Extensions.Logging;
using MrHell.Items.Base;
using PixelPilot.Common.Logging;

namespace MrHell.Items;

/// <summary>
/// Singleton ItemManager that is used to retrieve items.
/// Houses all possible items.
/// </summary>
public class ItemManager
{
    private ILogger _logger = LogManager.GetLogger("ItemManager");
    public static ItemManager Instance { get; private set; } = null!;
    
    public HellItem GetItem(string itemId) => Instance._items[itemId];
    public HellItem? GetItemByName(string itemName) => Instance._items.Values.FirstOrDefault(i => string.Equals(i.Name, itemName, StringComparison.OrdinalIgnoreCase));
    public List<HellItem> Items => _items.Values.ToList();

    private Dictionary<string, HellItem> _items = new();

    public ItemManager()
    {
        if (Instance != null) throw new Exception("An instance of ItemManager is already registered.");
        Instance = this;
    }

    public void RegisterItem(HellItem item)
    {
        if (!_items.TryAdd(item.Id, item)) throw new Exception("Item with this id already registered. (Id: ${item.Id})");
    }

    public void RegisterAllItems()
    {
        // Get the current assembly
        var assembly = Assembly.GetExecutingAssembly();

        // Find all types that inherit from HellItem
        var hellItemTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(HellItem)))
            .ToList();

        // Create instances of each HellItem subclass
        foreach (var type in hellItemTypes)
        {
            // Create instance using Activator
            var instance = (HellItem) Activator.CreateInstance(type)!;
            RegisterItem(instance);
        }
        
        _logger.LogInformation("Registering items: {Items}", string.Join(", ", _items.Select(i => i.Key)));
    }
    
}