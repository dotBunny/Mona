namespace Mona;

public static class Log
{
    public static void Clear()
    {
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    
    public static void Variable(string variableName, string value)
    {
        Console.WriteLine($"{GetPrefix()} [{variableName}] {value}");
    }
    public static void Message(string message)
    {
        Console.WriteLine($"{GetPrefix()} {message}");
    }

    public static void Error(string message)
    {
        Console.WriteLine($"{GetPrefix()} ERROR {message}");
    }

    private static string GetPrefix()
    {
        return $"[MONA @ {DateTime.Now.ToLongTimeString()}] ";
    }
}