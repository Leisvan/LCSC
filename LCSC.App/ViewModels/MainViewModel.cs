using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using LCSC.App.Models.Messages;
using LCTWorks.WinUI.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace LCSC.App.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private bool _activated;

    public MainViewModel()
    {
        Messenger.Register<MainViewModel, LoadingChangedMessage>(this, (recipient, message) =>
        {
            IsEnabled = !message.Value;
        });
    }

    [ObservableProperty]
    public partial string? AppVersionNotification { get; set; }

    public bool IsBotViewEnabled { get; } = true;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    public bool IsMembersViewEnabled { get; } = true;

    public bool IsTournamentsViewEnabled { get; } = true;

    public static StoreContext? GetStoreContext()
    {
        try
        {
            var w = App.MainWindow;
            var exApp = Application.Current.AsAppExtended();
            if (exApp == null || exApp.MainWindow == null)
            {
                return null;
            }
            //This here, throws a Win32 Unknown exception. No idea why.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(exApp.MainWindow);
            var storeContext = StoreContext.GetDefault();
            WinRT.Interop.InitializeWithWindow.Initialize(storeContext, hWnd);
            return storeContext;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public void Activated(object _, WindowActivatedEventArgs __)
    {
        if (_activated)
        {
            return;
        }
        CheckForUpdatesAsync();
    }

    private async void CheckForUpdatesAsync()
    {
        _activated = true;
        var context = GetStoreContext();
        if (context == null)
        {
            await UpdateVersionNotificationAsync("No store context found");
            AppVersionNotification = RuntimePackageHelper.IsDebug() ? "Debug" : RuntimePackageHelper.GetPackageVersion();
            return;
        }
        await UpdateVersionNotificationAsync("Store context found");

        //Is update available?
        var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
        if (updates.Count > 0)
        {
            var first = updates[0];
            var version = first.Package.Id.Version.ToVersionString();
            await UpdateVersionNotificationAsync($"Update found: {version}");
            await UpdateVersionNotificationAsync($"Trying to download update...", 200);

            var progress = new Progress<StorePackageUpdateStatus>(status =>
            {
                var progressPercent = status.PackageDownloadProgress * 100;
                AppVersionNotification = status.PackageUpdateState switch
                {
                    StorePackageUpdateState.Pending => $"Pending: {progressPercent:F0}%",
                    StorePackageUpdateState.Downloading => $"Downloading: {progressPercent:F0}%",
                    StorePackageUpdateState.Deploying => $"Installing: {progressPercent:F0}%",
                    StorePackageUpdateState.Completed => "Update completed!",
                    StorePackageUpdateState.Canceled => "Update canceled",
                    StorePackageUpdateState.ErrorLowBattery => "Error: Low battery",
                    StorePackageUpdateState.ErrorWiFiRecommended => "Error: WiFi recommended",
                    StorePackageUpdateState.ErrorWiFiRequired => "Error: WiFi required",
                    _ => $"Updating... {progressPercent:F0}%"
                };
            });

            var result = await context.RequestDownloadAndInstallStorePackageUpdatesAsync(updates)
                .AsTask(progress);

            if (result.OverallState == StorePackageUpdateState.Completed)
            {
                await UpdateVersionNotificationAsync("Update installed successfully!");
            }
        }

        //End:
        AppVersionNotification = RuntimePackageHelper.IsDebug() ? "Debug" : RuntimePackageHelper.GetPackageVersion();
    }

    private async Task UpdateVersionNotificationAsync(string? notificationText, int delayInMs = 2000)
    {
        AppVersionNotification = notificationText;
        await Task.Delay(delayInMs);
    }
}