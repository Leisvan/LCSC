using LCSC.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using WinUIEx;

namespace LCSC.App
{
    public sealed partial class MainWindow : WindowEx
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