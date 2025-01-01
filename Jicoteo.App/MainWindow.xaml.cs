using LCSC.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace LCSC.App
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