using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Models;

namespace OptimeGBAServer.Controllers
{
    [Route("/consoleInterface.sock")]
    public class ConsoleInterfaceController : AbstractJsonWebSocketController<ConsoleInterfaceRequest>
    {
        private readonly ILogger _logger;
        private readonly ScreenSubjectService _screen;

        public ConsoleInterfaceController(ILogger<ConsoleInterfaceController> logger, ScreenSubjectService screen)
            : base(logger, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        {
            _logger = logger;
            _screen = screen;
        }

        protected override async Task Handle(ConsoleInterfaceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handle action: {0}", request.Action);
            await Task.CompletedTask;
        }

        protected override async Task SendWorker(WebSocket webSocket, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                SendScreen(webSocket, cancellationToken)
            );
        }

        private async Task SendScreen(WebSocket webSocket, CancellationToken cancellationToken)
        {
            Channel<ReadOnlyMemory<byte>> bufferChannel = Channel.CreateBounded<ReadOnlyMemory<byte>>(
                new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropNewest }
            );

            try
            {
                _screen.RegisterObserver(bufferChannel.Writer);
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadOnlyMemory<byte> buffer = await bufferChannel.Reader.ReadAsync(cancellationToken);
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken);
                }
            }
            // Graceful close
            catch (OperationCanceledException) { }
            finally
            {
                _screen.DeregisterObserver(bufferChannel.Writer);
            }
        }
    }
}
