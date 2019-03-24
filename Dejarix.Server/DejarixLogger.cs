using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dejarix.Server
{
    public class DejarixLoggerProvider : ILoggerProvider
    {
        private readonly Channel<string> _channel;

        public ChannelReader<string> Reader => _channel.Reader;

        public DejarixLoggerProvider()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            };

            _channel = Channel.CreateUnbounded<string>(options);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DejarixLogger(_channel.Writer);
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
        }
    }

    public class DejarixLoggerService : IHostedService
    {
        private Task _task = null;
        private readonly DejarixLoggerProvider _provider;

        private async Task ConsumeAsync()
        {
            string now = DateTime.Now.ToString("s");
            string path = $"ignore.me.sql-{now}.txt";
            var reader = _provider.Reader;
            while (await reader.WaitToReadAsync())
            {
                using (var writer = new StreamWriter(path, true))
                {
                    while (reader.TryRead(out string item))
                        await writer.WriteLineAsync(item);

                    await writer.FlushAsync();
                    long logSize = writer.BaseStream.Position;
                    // TODO: Alert on large log file size.
                }
            }
        }

        public DejarixLoggerService(DejarixLoggerProvider provider)
        {
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _task = ConsumeAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _provider.Dispose();
            return _task ?? Task.CompletedTask;
        }
    }

    public class DejarixLogger : ILogger
    {
        private readonly ChannelWriter<string> _writer;

        public DejarixLogger(ChannelWriter<string> writer)
        {
            _writer = writer;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var formatted = formatter(state, exception);
            _writer.TryWrite(formatted);
        }
    }
}