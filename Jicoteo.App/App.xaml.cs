using LCSC.App.Services;
using LCSC.App.ViewModels;
using LCSC.Manager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LCSC.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        public new static App Current => (App)Application.Current;

        public static Window MainWindow { get; } = new MainWindow();

        public IServiceProvider Services { get; }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow.Activate();
        }

        private static ServiceProvider ConfigureServices()
            => new ServiceCollection()
            //Services
            .AddSingleton<AirtableService>()
            .AddSingleton<BotService>()
            .AddSingleton<MessageHandlingService>()
            //ViewModels
            .AddSingleton<MainViewModel>()
            .AddTransient<BotViewModel>()
            .AddTransient<MembersViewModel>()
            .AddTransient<TournamentsViewModel>()
            .BuildServiceProvider();
    }
}