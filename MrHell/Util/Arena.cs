namespace MrHell.Util;

public static class Arena
{
    public const int StartX = 14;
    public const int EndX = 35;
    public const int StartY = 19;
    public const int EndY = 30;

    public const int Width = EndX - StartX + 1;
    public const int Height = EndY - StartY + 1;

    public static bool InArena(int x, int y)
    {
        return x >= StartX && x <= EndX && y >= StartY && y <= EndY;
    }
}