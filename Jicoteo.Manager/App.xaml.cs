using Jicoteo.Manager.Helpers;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Jicoteo.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string BotConfigFileName = "dconfig.json";
        private const string CONFIGDirectoryName = "Bot\\Config";

        private static DependencyServiceHelper _dependencies = new DependencyServiceHelper();

        public static DependencyServiceHelper Dependencies => _dependencies;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}