namespace LCSC.Core.Models;

public record class RegionUpdateProgressReportData(int Number, int Total, string? EntryName, string? MessageText = null)
{
    public static RegionUpdateProgressReportData Message(string message)
        => new(0, 0, string.Empty, message);
}