namespace MrHell.Util;

public class MoveTicks
{
    private static int Ticks { get; set; } = 20;
    public static int Next() => Ticks++;
}