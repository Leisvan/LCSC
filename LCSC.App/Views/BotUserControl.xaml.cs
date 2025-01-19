using LCSC.App.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace LCSC.App.Views;

public sealed partial class BotUserControl : UserControl
{
    public BotUserControl()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<DiscordBotViewModel>();
    }

    public DiscordBotViewModel? ViewModel { get; }
}