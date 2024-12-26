using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.App.Helpers
{
    public static class DialogHelper
    {
        public static Task<ContentDialogResult> ShowDialogAsync(
            string title,
            string content,
            string primaryButtonText,
            string? secondaryButtonText = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
            };
            return ShowDialogAsync(dialog);
        }

        private static async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
        {
            var xamlRoot = App.MainWindow?.Content?.XamlRoot;
            if (xamlRoot == null)
            {
                return ContentDialogResult.None;
            }
            dialog.XamlRoot = xamlRoot;
            dialog.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
            return await dialog.ShowAsync();
        }
    }
}