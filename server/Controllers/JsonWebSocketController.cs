using System;
using System.IO;
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
    public abstract class JsonWebSocketController : ControllerBase
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
                WebSocketReadStream messageStream = new WebSocketReadStream(webSocket);
                byte[] actionBuffer = new byte[1];
                while (!cancellationToken.IsCancellationRequested)
                {
                    messageStream.Reset();
                    var actionFrame = await webSocket.ReceiveAsync(actionBuffer, cancellationToken);
                    char action = (char)actionBuffer[0];
                    if (actionFrame.EndOfMessage)
                    {
                        await HandleRequest(null, action, cancellationToken);
                    }
                    else
                    {
                        await HandleRequest(messageStream, action, cancellationToken);
                    }
                }
            }
            // Graceful close
            catch (WebSocketClosedException)
            {
                _logger.LogDebug("Client closed.");
            }
            catch (WebSocketException)
            {
                _logger.LogDebug("Client aborted.");
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

        protected abstract ValueTask HandleRequest(Stream? utf8JsonStream, char action, CancellationToken cancellationToken);

        protected abstract Task SendWorker(WebSocket webSocket, CancellationToken cancellationToken);
    }
}
