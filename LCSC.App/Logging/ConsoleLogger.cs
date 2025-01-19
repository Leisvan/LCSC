using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.App.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly string _categoryName;

        public ConsoleLogger(string categoryName)

        {
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var logMessage = formatter(state, exception);
            Console.WriteLine($"[{DateTime.Now}] {_categoryName} [{logLevel}]: {logMessage}");
        }
    }

    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(categoryName);
        }

        public void Dispose()
        { }
    }
}