using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace LCSC.App.Helpers;

public static class ClipboardHelper
{
    public static bool Copy(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public static async Task<string> GetCopiedTextAsync()
    {
        DataPackageView dataPackageView = Clipboard.GetContent();
        if (dataPackageView.Contains(StandardDataFormats.Text))
        {
            return await dataPackageView.GetTextAsync();
        }
        return string.Empty;
    }
}