using LCSC.App.Services;
using LCSC.App.ViewModels;
using LCSC.Core.Services;
using LCSC.Http.Services;
using LCSC.Manager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage;
using System;
using System.IO;
using Windows.ApplicationModel;


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
            var configuration = ReadConfigurations();
            Services = ConfigureServices(configuration);
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

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
            => new ServiceCollection()
            //Services
            .AddSingleton<PulseHttpService>()
            .AddSingleton(new AirtableHttpService(configuration["AirBaseSettings:token"], configuration["AirBaseSettings:baseId"]))
            .AddSingleton(new BattleNetHttpService(configuration["BattleNetSettings:clientId"], configuration["BattleNetSettings:clientSecret"]))
            .AddSingleton(new CacheService(ApplicationData.GetDefault().LocalCachePath))
            .AddSingleton<MembersService>()
            .AddSingleton<BotService>()
            .AddSingleton<MessageHandlingService>()
            //ViewModels
            .AddSingleton<MainViewModel>()
            .AddTransient<BotViewModel>()
            .AddTransient<MembersViewModel>()
            .AddTransient<TournamentsViewModel>()
            .BuildServiceProvider();

        private IConfiguration ReadConfigurations()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Package.Current.InstalledLocation.Path)
                .AddJsonFile("assets\\Config\\appsettings.json", false)
                .Build();
        }
    }
}