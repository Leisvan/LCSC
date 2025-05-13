using LCSC.App.ViewModels;
using LCSC.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage;
using LCSC.Discord.Services;
using Microsoft.Extensions.Logging;
using System;
using Windows.ApplicationModel;
using LCSC.Discord.Extensions;
using LCSC.App.Logging;
using LCTWorks.Common.Services.Telemetry;
using LCTWorks.Common.WinUI;
using LCSC.App.Helpers;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LCSC.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly ITelemetryService? _telemetryService;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            var configuration = ReadConfigurations();
            Services = ConfigureServices(configuration);
            this.InitializeComponent();

            _telemetryService = Services.GetService<ITelemetryService>();
            UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
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
            .AddSingleton(new CacheService(ApplicationData.GetDefault().LocalCachePath))
            .AddSingleton(sp => new LadderService(sp.GetRequiredService<CacheService>(), configuration["BattleNetSettings:clientId"], configuration["BattleNetSettings:clientSecret"]))
            .AddSingleton(sp => new MembersService(sp.GetRequiredService<LadderService>(), configuration["AirBaseSettings:token"], configuration["AirBaseSettings:baseId"]))

            //Discord bot
            .AddLogging(config =>
            {
                config.AddConsole();
                config.AddProvider(new ConsoleLoggerProvider());
            })
            .AddSingleton<DiscordBotService>()
            .ConfigureDiscordClient(configuration["DiscordSettings:token"])

            //ViewModels
            .AddSingleton<MainViewModel>()
            .AddTransient<DiscordBotViewModel>()
            .AddTransient<MembersViewModel>()
            .AddTransient<TournamentsViewModel>()

            //Telemetry
            .AddSentry(configuration["TelemetryKey:key"] ?? string.Empty, AppHelper.GetEnvironment(), AppHelper.IsDebug(), EnvironmentHelper.GetTelemetryContextData())

            //Build:
            .BuildServiceProvider(true);

        private static IConfiguration ReadConfigurations()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Package.Current.InstalledLocation.Path)
                .AddJsonFile("assets\\Config\\appsettings.json", false)
                .Build();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
            => _telemetryService?.ReportUnhandledException(e.Exception);

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
            => _telemetryService?.ReportUnhandledException(e.ExceptionObject as Exception ?? new Exception("Unknown exception"));

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs? e)
        {
            if (e?.Exception == null)
            {
                return;
            }
            var flattenedExceptions = e.Exception.Flatten().InnerExceptions;
            foreach (var exception in flattenedExceptions)
            {
                _telemetryService?.LogAndTrackError(GetType(), exception);
            }
            e.SetObserved();
        }
    }
}