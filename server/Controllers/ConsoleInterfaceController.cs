using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OptimeGBA;
using OptimeGBAServer.Models;

namespace OptimeGBAServer.Controllers
{
    [Route("/consoleInterface.sock")]
    public class ConsoleInterfaceController : AbstractJsonWebSocketController<ConsoleInterfaceRequest>
    {
        private const string KEY_ACTION_UP = "up";
        private const string KEY_ACTION_DOWN = "down";
        private const string KEY_A = "A";
        private const string KEY_B = "B";
        private const string KEY_L = "L";
        private const string KEY_R = "R";
        private const string KEY_SELECT = "select";
        private const string KEY_START = "start";
        private const string KEY_LEFT = "left";
        private const string KEY_RIGHT = "right";
        private const string KEY_UP = "up";
        private const string KEY_DOWN = "down";

        private readonly ScreenSubjectService _screen;
        private readonly GbaHostService _gba;

        private int _frameToken = 10;

        private readonly ConcurrentQueue<ReadOnlyMemory<byte>> _responseQueue = new ConcurrentQueue<ReadOnlyMemory<byte>>();

        public ConsoleInterfaceController(ILogger<ConsoleInterfaceController> logger, ScreenSubjectService screen, GbaHostService gba)
            : base(logger, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        {
            _screen = screen;
            _gba = gba;
        }

        protected override async Task Handle(ConsoleInterfaceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle action: {0}", request.Action);
            switch (request.Action)
            {
                case "key":
                    if (request.KeyAction is null)
                    {
                        _logger.LogWarning("Expected non-null {0} but got null. Discarded.", nameof(request.KeyAction));
                        break;
                    }
                    HandleKeyAction(request.KeyAction);
                    break;

                case "fillToken":
                    if (request.FillTokenAction is null)
                    {
                        _logger.LogWarning("Expected non-null {0} but got null. Discarded.", nameof(request.FillTokenAction));
                        break;
                    }
                    HandleFillTokenAction(request.FillTokenAction);
                    break;

                case "ping":
                    Respond(
                        new ConsoleInterfaceResponse()
                        {
                            Action = "pong",
                            PongAction = new PongAction()
                            {
                                MadeAt = request.PingAction?.MadeAt ?? 0
                            }
                        }
                    );
                    break;

                default:
                    _logger.LogWarning("Unknown action: {0}. Discarded.", request.Action);
                    break;
            }
            await Task.CompletedTask;
        }

        private void HandleKeyAction(KeyAction keyAction)
        {
            Gba? gba = this._gba.Emulator;
            if (gba is null)
            {
                _logger.LogWarning("Emulator is not ready yet. Can't send key event now.");
                return;
            }

            bool pressed;
            switch (keyAction.Action)
            {
                case KEY_ACTION_UP:
                    pressed = false;
                    break;

                case KEY_ACTION_DOWN:
                    pressed = true;
                    break;

                default:
                    _logger.LogWarning("{0} must be either \"up\" or \"down\". Discarded.", nameof(keyAction.Action));
                    return;
            }

            switch (keyAction.Key)
            {
                case KEY_A:
                    gba.Keypad.A = pressed;
                    break;

                case KEY_B:
                    gba.Keypad.B = pressed;
                    break;

                case KEY_L:
                    gba.Keypad.L = pressed;
                    break;

                case KEY_R:
                    gba.Keypad.R = pressed;
                    break;

                case KEY_SELECT:
                    gba.Keypad.Select = pressed;
                    break;

                case KEY_START:
                    gba.Keypad.Start = pressed;
                    break;

                case KEY_LEFT:
                    gba.Keypad.Left = pressed;
                    break;

                case KEY_RIGHT:
                    gba.Keypad.Right = pressed;
                    break;

                case KEY_UP:
                    gba.Keypad.Up = pressed;
                    break;

                case KEY_DOWN:
                    gba.Keypad.Down = pressed;
                    break;

                default:
                    _logger.LogWarning("Unknown key {0}. Discarded.", keyAction.Key);
                    return;
            }
        }

        private void HandleFillTokenAction(FillTokenAction fillTokenAction)
        {
            Interlocked.Add(ref this._frameToken, fillTokenAction.Count);
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
                    if (_frameToken > 0)
                    {
                        Interlocked.Decrement(ref _frameToken);
                        await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken);
                    }

                    while (_responseQueue.TryDequeue(out ReadOnlyMemory<byte> response))
                    {
                        await webSocket.SendAsync(response, WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }
            // Graceful close
            catch (OperationCanceledException) { }
            finally
            {
                _screen.DeregisterObserver(bufferChannel.Writer);
            }
        }

        private void Respond(ConsoleInterfaceResponse response)
        {
            _responseQueue.Enqueue(JsonSerializer.SerializeToUtf8Bytes(response, _serializerOptions));
        }
    }
}
