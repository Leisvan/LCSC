using Jicoteo.Bot;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jicoteo.Host.Helpers
{
    public class DependencyServiceHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _services;

        public DependencyServiceHelper()
        {
            _services = new ServiceCollection()
                           //Bot
                           .AddSingleton<BotManager>()
                           .AddSingleton<MessageListener>()
                           //
                           .AddLogging();

            var container = new Container();
            container.Configure(config =>
            {
                config.Scan(_ =>
                {
                    _.AssemblyContainingType(typeof(Program));
                    _.WithDefaultConventions();
                });
                config.Populate(_services);
            });

            _serviceProvider = container.GetInstance<IServiceProvider>();
        }

        public IServiceProvider ServiceProvider => _serviceProvider;

        public IServiceCollection Services => _services;
    }
}