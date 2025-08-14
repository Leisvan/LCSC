using LCSC.App.Helpers;
using LCSC.App.Logging;
using LCSC.App.ViewModels;
using LCSC.Core.Helpers;
using LCSC.Core.Services;
using LCSC.Discord.Extensions;
using LCSC.Discord.Services;
using LCTWorks.Common.Services.Telemetry;
using LCTWorks.Common.WinUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace LCSC.App
{
    public partial class App : Application
    {
        private readonly ITelemetryService? _telemetryService;

        public App()
        {
            var configuration = ReadConfigurations();
            Services = ConfigureServices(configuration);
            BuildHelper.IsDebugBuild = AppHelper.IsDebug();
            this.InitializeComponent();

            _telemetryService = Services.GetService<ITelemetryService>();
            UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public new static App Current => (App)Application.Current;

        public static Window MainWindow { get; } = new MainWindow();

        public IServiceProvider Services { get; }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow.Activate();
        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
            => new ServiceCollection()
            //Services
            .AddSingleton(new CacheService(ApplicationData.GetDefault().LocalCachePath))
            .AddSingleton(sp => new LadderService(sp.GetRequiredService<CacheService>(), configuration["BattleNetSettings:clientId"], configuration["BattleNetSettings:clientSecret"]))
            .AddSingleton(sp => new CommunityDataService(sp.GetRequiredService<LadderService>(), sp.GetRequiredService<CacheService>(), configuration["AirBaseSettings:token"], configuration["AirBaseSettings:baseId"], Package.Current.InstalledLocation.Path))

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
            .AddSentry(configuration["TelemetryKey:key"] ?? string.Empty, AppHelper.GetEnvironment(), BuildHelper.IsDebugBuild, EnvironmentHelper.GetTelemetryContextData())

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