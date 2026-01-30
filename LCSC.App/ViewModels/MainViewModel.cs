using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LCSC.App.Helpers;
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
        if (AppHelper.IsDebug())
        {
            await UpdateVersionNotificationAsync();
            return;
        }

        await UpdateVersionNotificationAsync("AppUpdates-CheckingForUpdates".GetTextLocalized());
        var context = GetStoreContext();
        if (context == null)
        {
            await UpdateVersionNotificationAsync("AppUpdates-StoreContextError".GetTextLocalized());
            await UpdateVersionNotificationAsync();
            return;
        }

        var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
        if (updates.Count > 0)
        {
            var first = updates[0];
            var version = first.Package.Id.Version.ToVersionString();
            await UpdateVersionNotificationAsync($"AppUpdates-UpdateFound".GetTextLocalized());
            var progress = new Progress<StorePackageUpdateStatus>(status =>
            {
                var progressPercent = status.PackageDownloadProgress * 100;
                var progress = progressPercent.ToString("F0");
                AppVersionNotification = status.PackageUpdateState switch
                {
                    StorePackageUpdateState.Pending => string.Format("AppUpdates-ProgressPending".GetTextLocalized(), progress),
                    StorePackageUpdateState.Downloading => string.Format("AppUpdates-ProgressDownloading".GetTextLocalized(), progress),
                    StorePackageUpdateState.Deploying => string.Format("AppUpdates-ProgressDeploying".GetTextLocalized(), progress),
                    StorePackageUpdateState.Completed => "AppUpdates-ProgressCompleted".GetTextLocalized(),
                    StorePackageUpdateState.Canceled => "AppUpdates-ProgressCanceled".GetTextLocalized(),
                    StorePackageUpdateState.ErrorLowBattery => "AppUpdates-ErrorLowBattery".GetTextLocalized(),
                    StorePackageUpdateState.ErrorWiFiRecommended => "AppUpdates-ErrorWiFiRecommended".GetTextLocalized(),
                    StorePackageUpdateState.ErrorWiFiRequired => "AppUpdates-ErrorWiFiRequired".GetTextLocalized(),
                    _ => string.Format("AppUpdates-ProgressUnknown".GetTextLocalized(), progress)
                };
            });

            var result = await context.RequestDownloadAndInstallStorePackageUpdatesAsync(updates)
                .AsTask(progress);

            if (result.OverallState != StorePackageUpdateState.Completed)
            {
                await UpdateVersionNotificationAsync("AppUpdates-ErrorInstallingUpdate".GetTextLocalized());
            }
        }

        //End:
        await UpdateVersionNotificationAsync();
    }

    private async Task UpdateVersionNotificationAsync(string? notificationText = null, int delayInMs = 2000)
    {
        if (notificationText == null)
        {
            notificationText = RuntimePackageHelper.IsDebug()
                ? "AppUpdates-DebugVersion".GetTextLocalized()
                : RuntimePackageHelper.GetPackageVersion();
        }
        AppVersionNotification = notificationText;
        await Task.Delay(delayInMs);
    }
}