using MrHell.Players;

namespace MrHell.Items.Base;

public abstract class HellItem
{
    protected HellItem(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
        
        if (name.Contains(' ')) throw new Exception("The name cannot contain spaces");
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public abstract Task Execute(HellPlayer player, IHellApi api);
}