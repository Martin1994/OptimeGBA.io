using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using OptimeGBAServer.Exceptions;

namespace OptimeGBAServer.Utilities
{
    public class WebSocketReadStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new InvalidOperationException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private bool _aboutToClose = false;
        private bool _closed = false;
        private readonly WebSocket _webSocket;

        public WebSocketReadStream(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public override void Flush() => throw new InvalidOperationException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_closed)
            {
                throw new EndOfStreamException();
            }

            if (_aboutToClose)
            {
                _closed = true;
                return 0;
            }

            var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Binary)
            {
                throw new NotSupportedException("Only test message in JSON format is supported.");
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new WebSocketClosedException();
            }

            if (result.EndOfMessage) {
                _aboutToClose = true;
            }

            return result.Count;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();

        public override void SetLength(long value) => throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
    }
}
