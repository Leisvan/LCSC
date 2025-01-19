namespace LCSC.Discord.Extensions;

internal static class StringExtensions
{
    public static string Format(this string str, params object[] args)
        => string.Format(str, args);
}