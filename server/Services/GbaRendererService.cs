using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimeGBA;
using OptimeGBAServer.Collections;

namespace OptimeGBAServer.Services
{
    public abstract class GbaRendererService<TScreenBuffer> : DaemonService, IGbaRenderer
    {
        private readonly WindowAverage _bpf = new WindowAverage(60);
        public double Bpf { get => _bpf.Average; }

        public abstract string CodecString { get; }

        private readonly VideoSubjectService _videoSubjectService;

        private readonly Channel<TScreenBuffer> _emulatorScreenBuffer = Channel.CreateUnbounded<TScreenBuffer>();
        private readonly Channel<TScreenBuffer> _rendererScreenBuffer = Channel.CreateUnbounded<TScreenBuffer>();

        public GbaRendererService(IHostApplicationLifetime lifetime, ILogger logger, VideoSubjectService videoSubjectService) : base(lifetime, logger)
        {
            _videoSubjectService = videoSubjectService;
            _emulatorScreenBuffer.Writer.TryWrite(this.ProvideScreenBuffer());
        }

        protected abstract TScreenBuffer ProvideScreenBuffer();

        public async ValueTask EnqueueFrame(Gba gba, CancellationToken cancellationToken)
        {
            TScreenBuffer screenBuffer = await _emulatorScreenBuffer.Reader.ReadAsync(cancellationToken);
            SnapshotScreen(gba, screenBuffer);
            await _rendererScreenBuffer.Writer.WriteAsync(screenBuffer, cancellationToken);
        }

        protected async ValueTask DequeueFrame(Action<TScreenBuffer> processFrame, CancellationToken cancellationToken)
        {
            TScreenBuffer screenBuffer = await _rendererScreenBuffer.Reader.ReadAsync(cancellationToken);
            processFrame(screenBuffer);
            await _emulatorScreenBuffer.Writer.WriteAsync(screenBuffer, cancellationToken);
        }

        protected abstract void SnapshotScreen(Gba gba, TScreenBuffer screenBuffer);

        protected void FlushFrame(VideoSubjectPayload payload)
        {
            _videoSubjectService.BufferWriter.TryWrite(payload);
            _bpf.AddSample(payload.Buffer.Length << 3);
        }
    }
}
