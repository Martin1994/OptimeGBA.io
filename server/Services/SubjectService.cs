using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptimeGBAServer.Services
{
    public abstract class SubjectService<TPayload> : DaemonService
    {
        private readonly ILogger _logger;

        private readonly Channel<TPayload> _buffer = Channel.CreateBounded<TPayload>(new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.DropOldest });
        public ChannelWriter<TPayload> BufferWriter { get => _buffer.Writer; }

        private readonly ConcurrentDictionary<ChannelWriter<TPayload>, bool> _observers = new ConcurrentDictionary<ChannelWriter<TPayload>, bool>();
        public int ObserverCount { get => _observers.Count; }

        public SubjectService(IHostApplicationLifetime lifetime, ILogger logger) : base(lifetime, logger)
        {
            _logger = logger;
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
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
