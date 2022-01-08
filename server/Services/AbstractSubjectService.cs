
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptimeGBAServer
{
    public abstract class AbstractSubjectService : IHostedService
    {

        private readonly ILogger _logger;

        private Task? _backgroundTask;
        private CancellationTokenSource? _backgroundCancellation;

        private readonly Channel<ReadOnlyMemory<byte>> _buffer = Channel.CreateBounded<ReadOnlyMemory<byte>>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropNewest });
        public ChannelWriter<ReadOnlyMemory<byte>> BufferWriter { get => _buffer.Writer; }

        private readonly ConcurrentDictionary<ChannelWriter<ReadOnlyMemory<byte>>, bool> _observers = new ConcurrentDictionary<ChannelWriter<ReadOnlyMemory<byte>>, bool>();
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
                ReadOnlyMemory<byte> buffer = await _buffer.Reader.ReadAsync(cancellationToken);

                // It it safe to iterate a ConcurrentDictionary while its content can be updated
                foreach (ChannelWriter<ReadOnlyMemory<byte>> observerWriter in _observers.Keys)
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

        public void RegisterObserver(ChannelWriter<ReadOnlyMemory<byte>> observerWriter)
        {
            _observers.TryAdd(observerWriter, true);
        }

        public void DeregisterObserver(ChannelWriter<ReadOnlyMemory<byte>> observerWriter)
        {
            _observers.TryRemove(observerWriter, out _);
        }
    }
}
