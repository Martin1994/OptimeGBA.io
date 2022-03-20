using System;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Exceptions;
using OptimeGBAServer.IO;

namespace OptimeGBAServer.Controllers
{
    public abstract class JsonWebSocketController<TRequest> : ControllerBase where TRequest : class
    {
        protected readonly ILogger _logger;
        protected readonly JsonSerializerOptions? _serializerOptions;

        public JsonWebSocketController(ILogger logger, JsonSerializerOptions? serializerOptions = null)
        {
            _logger = logger;
            _serializerOptions = serializerOptions;
        }

        [HttpGet]
        public async Task Get(CancellationToken cancellationToken)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(new WebSocketAcceptContext() { DangerousEnableCompression = true });
                var endCancellationSource = new CancellationTokenSource();
                var sendCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, endCancellationSource.Token);
                await Task.WhenAll(
                    Receive(webSocket, endCancellationSource, cancellationToken),
                    SendWorker(webSocket, sendCancellationSource.Token)
                );
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task Receive(WebSocket webSocket, CancellationTokenSource endCancellationSource, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        WebSocketReadStream messageStream = new WebSocketReadStream(webSocket);
                        TRequest? request = await JsonSerializer.DeserializeAsync<TRequest>(messageStream, _serializerOptions, cancellationToken);
                        if (request is null)
                        {
                            _logger.LogDebug("Received null. Skipped.");
                            continue;
                        }
                        await HandleRequest(request, cancellationToken);
                    }
                    catch (JsonException)
                    {
                        _logger.LogDebug("Received invalid JSON. Skipped.");
                    }
                }
            }
            // Graceful close
            catch (WebSocketClosedException)
            {
                _logger.LogDebug("Client closed.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Client aborted.");
            }
            finally
            {
                endCancellationSource.Cancel();
            }
        }

        protected abstract Task HandleRequest(TRequest request, CancellationToken cancellationToken);

        protected abstract Task SendWorker(WebSocket webSocket, CancellationToken cancellationToken);
    }
}
