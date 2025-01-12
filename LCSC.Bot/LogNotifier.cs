namespace LCSC.Discord
{
    internal static class LogNotifier
    {
        public static void Notify(string message)
            => Console.WriteLine(message);

        public static void NotifyError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}