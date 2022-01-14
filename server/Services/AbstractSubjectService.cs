
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptimeGBAServer
{
    public abstract class AbstractSubjectService<TPayload> : IHostedService
    {

        private readonly ILogger _logger;

        private Task? _backgroundTask;
        private CancellationTokenSource? _backgroundCancellation;

        private readonly Channel<TPayload> _buffer = Channel.CreateBounded<TPayload>(new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.DropOldest });
        public ChannelWriter<TPayload> BufferWriter { get => _buffer.Writer; }

        private readonly ConcurrentDictionary<ChannelWriter<TPayload>, bool> _observers = new ConcurrentDictionary<ChannelWriter<TPayload>, bool>();
        public int ObserverCount { get => _observers.Count; }

        public AbstractSubjectService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting up...");

            _backgroundCancellation = new CancellationTokenSource();
            _backgroundTask =  Task.Factory.StartNew(async () => await RunAsync(_backgroundCancellation.Token), creationOptions: TaskCreationOptions.LongRunning);

            await Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TPayload buffer = await _buffer.Reader.ReadAsync(cancellationToken);

                // It it safe to iterate a ConcurrentDictionary while its content can be updated
                foreach (ChannelWriter<TPayload> observerWriter in _observers.Keys)
                {
                    observerWriter.TryWrite(buffer);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tearing down...");

            _backgroundCancellation?.Cancel();
            await (_backgroundTask ?? Task.CompletedTask);
        }

        public void RegisterObserver(ChannelWriter<TPayload> observerWriter)
        {
            _observers.TryAdd(observerWriter, true);
        }

        public void DeregisterObserver(ChannelWriter<TPayload> observerWriter)
        {
            _observers.TryRemove(observerWriter, out _);
        }
    }
}
