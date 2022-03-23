using OptimeGBA;
using OptimeGBAServer.Media;
using OptimeGBAServer.Media.LibOpenH264;
using OptimeGBAServer.Media.LibOpenH264.Native;
using OptimeGBAServer.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static OptimeGBAServer.Services.GbaHostService;
using static OptimeGBAServer.Media.LibOpenH264.Native.ENCODER_OPTION;
using static OptimeGBAServer.Media.LibOpenH264.Native.EVideoFrameType;
using static OptimeGBAServer.Media.LibOpenH264.Native.EVideoFormatType;
using static OptimeGBAServer.Media.LibOpenH264.Native.EUsageType;

namespace OptimeGBAServer.Services
{
    public class H264RendererService : GbaRendererService<OpenH264SourcePicture>
    {
        private readonly ILogger _logger;

        private readonly ScreenshotHelper _screenshot;

        public override string CodecString => "avc1.64001f";

        public H264RendererService(
            IHostApplicationLifetime lifetime, IConfiguration configuration, ILogger<H264RendererService> logger,
            ScreenSubjectService screenSubjectService, ScreenshotHelper screenshot
        ) : base(lifetime, logger, screenSubjectService)
        {
            _logger = logger;
            _screenshot = screenshot;
        }

        protected override void SnapshotScreen(Gba gba, OpenH264SourcePicture image)
        {
            Debug.Assert(image.PicWidth == GbaHostService.GBA_WIDTH);
            Debug.Assert(image.PicHeight == GbaHostService.GBA_HEIGHT);
            Debug.Assert(image.ColorFormat == videoFormatI420);

            int colorMask = ScreenshotHelper.COLOR_MASK;
            byte[] yLut = _screenshot.YLut;
            byte[] uLut = _screenshot.ULut;
            byte[] vLut = _screenshot.VLut;

            int yStride = image.Stride[0];
            int uStride = image.Stride[1];
            int vStride = image.Stride[2];

            Span<byte> yData = image.GetData(0);
            Span<byte> uData = image.GetData(1);
            Span<byte> vData = image.GetData(2);

            Span<ushort> screen = gba.Ppu.Renderer.ScreenFront;
            for (int j = 0; j < GbaHostService.GBA_HEIGHT; j++)
            {
                int indexY = 0;
                int indexU = 0;
                int indexV = 0;
                Span<byte> rowY = yData.Slice(j * yStride, yStride);
                Span<byte> rowU = uData.Slice((j >> 1) * uStride, uStride);
                Span<byte> rowV = vData.Slice((j >> 1) * vStride, vStride);

                for (int i = 0; i < GbaHostService.GBA_WIDTH; i++)
                {
                    int rgb555 = screen[i + j * GbaHostService.GBA_WIDTH] & colorMask;
                    byte y = yLut[rgb555];
                    rowY[indexY++] = y;

                    if ((i & 1) == 0 && (j & 1) == 0)
                    {
                        byte u = uLut[rgb555];
                        byte v = vLut[rgb555];
                        rowU[indexU++] = u;
                        rowV[indexV++] = v;
                    }
                }
            }
        }

        protected override OpenH264SourcePicture ProvideScreenBuffer()
        {
            return new OpenH264SourcePictureI420(GBA_WIDTH, GBA_HEIGHT);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            using OpenH264Encoder encoder = new OpenH264Encoder((ref TagEncParamExt config) =>
            {
                config.iPicWidth = GBA_WIDTH;
                config.iPicHeight = GBA_HEIGHT;
                config.iUsageType = SCREEN_CONTENT_REAL_TIME;
                config.fMaxFrameRate = 0f;
                config.iTargetBitrate = 262144;
                config.iMultipleThreadIdc = 1; // Off
            });

            EVideoFormatType videoFormat = videoFormatI420;
            encoder.SetOption(ENCODER_OPTION_DATAFORMAT, ref videoFormat);

            int bufferPoolSize = 16;
            int bufferSize = 0x20000; // 128k
            int bufferPoolIndex = 0;
            byte[] frameBuffer = new byte[bufferSize * bufferPoolSize];

            double timestamp = 0d;
            double timestampIncrement = 1d / 60d;
            int forceKeyframeCountdown = 0;
            OpenH264FrameBSInfo frame = new OpenH264FrameBSInfo();
            while (!cancellationToken.IsCancellationRequested)
            {
                forceKeyframeCountdown = (forceKeyframeCountdown + 1) % 120;
                if (forceKeyframeCountdown == 0)
                {
                    encoder.ForceIntraFrame(true);
                }

                void ProcessFrame(OpenH264SourcePicture screenBuffer)
                {
                    screenBuffer.TimeStamp = (long)timestamp;
                    encoder.EncodeFrame(screenBuffer, frame);
                    timestamp += timestampIncrement;
                }
                await DequeueFrame(ProcessFrame, cancellationToken);

                if (frame.FrameType != videoFrameTypeSkip)
                {
                    int bufferOffset = bufferPoolIndex * bufferSize;
                    int bufferOffsetStart = bufferOffset;
                    bufferPoolIndex = (bufferOffset + 1) % bufferPoolSize;
                    Memory<byte> buffer = new Memory<byte>(frameBuffer, bufferOffset, bufferSize);
                    frame.CopyFrameData(buffer.Span);
                    FlushFrame(new ScreenSubjectPayload()
                    {
                        Buffer = buffer.Slice(0, frame.FrameSizeInBytes),
                        FrameMetadata = new FrameMetadata()
                        {
                            IsKey = frame.FrameType == videoFrameTypeIDR
                        }
                    });
                }
            }
        }
    }
}
