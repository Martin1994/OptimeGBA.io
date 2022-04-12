using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptimeGBAServer.Services
{
    public abstract class DaemonService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;

        private Task? _backgroundTask;
        private CancellationTokenSource? _backgroundCancellation;

        public DaemonService(IHostApplicationLifetime lifetime, ILogger logger)
        {
            _lifetime = lifetime;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting up daemon...");

            _backgroundTask = RunAsyncAndWrapExceptions();

            await Task.CompletedTask;
        }

        private async Task RunAsyncAndWrapExceptions() {
            _backgroundCancellation = new CancellationTokenSource();
            try
            {
                await RunAsync(_backgroundCancellation.Token);
            }
            catch (OperationCanceledException) {}
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _lifetime.StopApplication();
            }
        }

        protected abstract Task RunAsync(CancellationToken cancellationToken);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tearing down daemon...");

            _backgroundCancellation?.Cancel();
            await (_backgroundTask ?? Task.CompletedTask);
        }
    }
}
