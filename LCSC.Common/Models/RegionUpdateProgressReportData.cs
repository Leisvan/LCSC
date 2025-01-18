namespace LCSC.Core.Models;

public record class RegionUpdateProgressReportData(int Number, int Total, string? EntryName, string? ErrorMessage = null)
{
    public static RegionUpdateProgressReportData FromError(string errorMessage)
        => new(0, 0, string.Empty, errorMessage);
}