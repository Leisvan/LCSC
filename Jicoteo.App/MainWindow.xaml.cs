using LSCC.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace LSCC.App
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<MainViewModel>();
            ExtendsContentIntoTitleBar = true;
        }

        public MainViewModel? ViewModel { get; }
    }
}