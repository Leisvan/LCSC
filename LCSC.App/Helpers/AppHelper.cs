namespace LCSC.App.Helpers;

public static class AppHelper
{
    public static string GetEnvironment()
    {
        return IsDebug() ? "Debug" : "Release";
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}