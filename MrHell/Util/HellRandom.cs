namespace MrHell.Util;

public class HellRandom
{
    public static Random Instance = new Random();
    public static int Next(int max) => Instance.Next(max);
    public static bool Bool() => Instance.NextDouble() > 0.5;
}