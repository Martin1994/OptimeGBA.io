using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OptimeGBA;
using OptimeGBAServer.Models;
using OptimeGBAServer.Services;

namespace OptimeGBAServer.Controllers
{
    [Route("/consoleInterface.sock")]
    public class ConsoleInterfaceController : JsonWebSocketController
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

        private readonly VideoSubjectService _video;
        private readonly AudioSubjectService _audio;
        private readonly GbaHostService _gba;
        private readonly IGbaRenderer _renderer;

        private int _frameToken = 10;

        private bool _receivedKeyFrame = false;

        private bool _mute = true;

        private readonly ConcurrentQueue<ReadOnlyMemory<byte>> _responseQueue = new ConcurrentQueue<ReadOnlyMemory<byte>>();

        public ConsoleInterfaceController(
            ILogger<ConsoleInterfaceController> logger,
            VideoSubjectService video, AudioSubjectService audio,
            GbaHostService gba, IGbaRenderer renderer
        ) : base(logger, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        {
            _video = video;
            _audio = audio;
            _gba = gba;
            _renderer = renderer;
        }

        protected override async ValueTask HandleRequest(Stream? utf8JsonStream, char action, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle action: {0}", action);
            try
            {
                switch (action)
                {
                    case 'k': // key
                        if (utf8JsonStream == null)
                        {
                            _logger.LogWarning("Expected JSON body but got nothing. Discarded.");
                            break;
                        }
                        HandleKeyRequest(await JsonSerializer.DeserializeAsync<KeyRequest>(utf8JsonStream, _serializerOptions, cancellationToken));
                        break;

                    case 't': // fill token
                        if (utf8JsonStream != null)
                        {
                            _logger.LogWarning("Expected no JSON body but got something. Discarded.");
                            await JsonSerializer.DeserializeAsync<DummyRequest>(utf8JsonStream, _serializerOptions, cancellationToken);
                            break;
                        }
                        Interlocked.Add(ref this._frameToken, 1);
                        break;

                    case 'p': // ping
                        if (utf8JsonStream == null)
                        {
                            _logger.LogWarning("Expected JSON body but got nothing. Discarded.");
                            break;
                        }
                        HandlePingRequest(await JsonSerializer.DeserializeAsync<PingRequest>(utf8JsonStream, _serializerOptions, cancellationToken));
                        break;

                    case 'a': // audio control
                        if (utf8JsonStream == null)
                        {
                            _logger.LogWarning("Expected JSON body but got nothing. Discarded.");
                            break;
                        }
                        HandleAudioControlRequest(await JsonSerializer.DeserializeAsync<AudioControlRequest>(utf8JsonStream, _serializerOptions, cancellationToken));
                        break;

                    default:
                        _logger.LogWarning("Unknown action: {0}. Discarded.", action);
                        break;
                }
            }
            catch (JsonException)
            {
                _logger.LogDebug("Received invalid JSON. Skipped.");
            }
            await Task.CompletedTask;
        }

        private void HandlePingRequest(PingRequest request)
        {
            Respond(new PongResponse()
            {
                MadeAt = request.MadeAt
            });
        }

        private void HandleAudioControlRequest(AudioControlRequest request)
        {
            this._mute = request.Mute;
        }

        private void HandleKeyRequest(KeyRequest request)
        {
            Gba? gba = this._gba.Emulator;
            if (gba == null)
            {
                _logger.LogWarning("Emulator is not ready yet. Can't send key event now.");
                return;
            }

            bool pressed;
            switch (request.Action)
            {
                case KEY_ACTION_UP:
                    pressed = false;
                    break;

                case KEY_ACTION_DOWN:
                    pressed = true;
                    break;

                default:
                    _logger.LogWarning("{0} must be either \"up\" or \"down\". Discarded.", nameof(request.Action));
                    return;
            }

            switch (request.Key)
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
                    _logger.LogWarning("Unknown key {0}. Discarded.", request.Key);
                    return;
            }
        }

        protected override async Task SendWorker(WebSocket webSocket, CancellationToken cancellationToken)
        {
            Respond(new InitResponse()
            {
                Codec = _renderer.CodecString
            });

            var videoBufferChannel = Channel.CreateBounded<VideoSubjectPayload>(
                new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropNewest }
            );
            _video.RegisterObserver(videoBufferChannel.Writer);

            var audioBufferChannel = Channel.CreateBounded<AudioSubjectPayload>(
                new BoundedChannelOptions(128) { FullMode = BoundedChannelFullMode.DropNewest }
            );
            _audio.RegisterObserver(audioBufferChannel.Writer);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.WhenAny(
                        videoBufferChannel.Reader.WaitToReadAsync(cancellationToken).AsTask(),
                        audioBufferChannel.Reader.WaitToReadAsync(cancellationToken).AsTask()
                    );
                    await SendAllResponses(webSocket, cancellationToken);
                    await SendVideo(webSocket, videoBufferChannel.Reader, cancellationToken);
                    await SendAudio(webSocket, audioBufferChannel.Reader, cancellationToken);
                }
            }
            // Graceful close
            catch (OperationCanceledException) { }
            finally
            {
                _video.DeregisterObserver(videoBufferChannel.Writer);
                _audio.DeregisterObserver(audioBufferChannel.Writer);
            }
        }

        private async ValueTask SendVideo(WebSocket webSocket, ChannelReader<VideoSubjectPayload> bufferReader, CancellationToken cancellationToken)
        {
            while (bufferReader.TryRead(out VideoSubjectPayload payload))
            {
                // A simple traffic control. Client will send back a request whenever a frame is received.
                if (_frameToken > 0)
                {
                    if (_receivedKeyFrame || payload.FrameMetadata.IsKey)
                    {
                        Interlocked.Decrement(ref _frameToken);
                        if (!_receivedKeyFrame)
                        {
                            _receivedKeyFrame = true;
                        }
                        await webSocket.SendAsync(payload.Buffer, WebSocketMessageType.Binary, true, cancellationToken);
                    }
                }
            }
        }

        private async ValueTask SendAudio(WebSocket webSocket, ChannelReader<AudioSubjectPayload> bufferReader, CancellationToken cancellationToken)
        {
            while (bufferReader.TryRead(out AudioSubjectPayload payload))
            {
                // Share the same traffic control with the video stream.
                if (!_mute && _frameToken > 0)
                {
                    await webSocket.SendAsync(payload.Buffer, WebSocketMessageType.Binary, true, cancellationToken);
                }
            }
        }

        private async ValueTask SendAllResponses(WebSocket webSocket, CancellationToken cancellationToken)
        {
            while (_responseQueue.TryDequeue(out ReadOnlyMemory<byte> response))
            {
                await webSocket.SendAsync(response, WebSocketMessageType.Text, true, cancellationToken);
            }
        }

        private void Respond<T>(T response) where T : ConsoleInterfaceResponse
        {
            _responseQueue.Enqueue(JsonSerializer.SerializeToUtf8Bytes(response, _serializerOptions));
        }
    }
}
