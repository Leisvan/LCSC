using LCSC.Core.Interactivity;

namespace LCSC.Discord
{
    internal static class LogNotifier
    {
        public static void Notify(string message)
            => Console.WriteLine(message);

        public static void NotifyError(string message)
            => ConsoleInteractionsHelper.WriteLine(message, ConsoleColor.Red);
    }
}