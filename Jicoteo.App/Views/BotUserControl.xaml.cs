using LSCC.App.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace LSCC.App.Views;

public sealed partial class BotUserControl : UserControl
{
    public BotUserControl()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<BotViewModel>();
    }

    public BotViewModel? ViewModel { get; }
}