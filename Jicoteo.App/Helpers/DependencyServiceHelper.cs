using Jicoteo.Manager.Bot;
using Jicoteo.Manager;
using Jicoteo.Manager.Services;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using System;

namespace Jicoteo.Manager.Helpers
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
                //config.Scan(_ =>
                //{
                //    _.AssemblyContainingType(typeof(Jicoteo.App.App));
                //    _.WithDefaultConventions();
                //});
                //config.Populate(_services);
            });

            _serviceProvider = container.GetInstance<IServiceProvider>();
        }

        public IServiceProvider ServiceProvider => _serviceProvider;

        public IServiceCollection Services => _services;
    }
}