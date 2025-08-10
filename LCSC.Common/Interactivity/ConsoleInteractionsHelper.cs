namespace LCSC.Core.Interactivity;

public static class ConsoleInteractionsHelper
{
    private static readonly Lock _lock = new();
    private static ConsoleColor _foregroundColor = ConsoleColor.Gray;

    public static ConsoleColor ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            lock (_lock)
            {
                _foregroundColor = value;
                Console.ForegroundColor = value;
            }
        }
    }

    public static void ResetForegroundColor()
        => ForegroundColor = ConsoleColor.Gray;

    public static void WriteLine(string message, ConsoleColor color)
    {
        ForegroundColor = color;
        Console.WriteLine(message);
        ResetForegroundColor();
    }
}