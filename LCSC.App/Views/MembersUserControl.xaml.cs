using LCSC.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace LCSC.App.Views;

public sealed partial class MembersUserControl : UserControl
{
    public MembersUserControl()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<MembersViewModel>();
    }

    public MembersViewModel? ViewModel { get; }
}