using CommunityToolkit.Mvvm.ComponentModel;
using LCSC.App.Models.Messages;
using CommunityToolkit.Mvvm.Messaging;
using LCTWorks.WinUI.Helpers;

namespace LCSC.App.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        Messenger.Register<MainViewModel, LoadingChangedMessage>(this, (recipient, message) =>
        {
            IsEnabled = !message.Value;
        });
    }

    public string AppVersion => RuntimePackageHelper.IsDebug() ? "Debug" : RuntimePackageHelper.GetPackageVersion();

    public bool IsBotViewEnabled => true;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    public bool IsMembersViewEnabled => true;

    public bool IsTournamentsViewEnabled => true;
}