using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimeGBA;
using OptimeGBAServer.Media;
using OptimeGBAServer.Media.LibVpx;
using OptimeGBAServer.Media.LibVpx.Native;
using OptimeGBAServer.Models;

using static OptimeGBAServer.Media.LibVpx.Native.vp8e_enc_control_id;
using static OptimeGBAServer.Media.LibVpx.Native.vp9e_tune_content;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_cx_pkt_kind;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_er_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_frame_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_enc_deadline_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_enc_frame_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_img_fmt_t;
using static OptimeGBAServer.Services.GbaHostService;

namespace OptimeGBAServer.Services
{
    public class Vp9RendererService : GbaRendererService<VpxImage>
    {
        private readonly ILogger _logger;

        private readonly ScreenshotHelper _screenshot;

        public override string CodecString => "vp09.01.10.08.03";

        public Vp9RendererService(
            IHostApplicationLifetime lifetime, IConfiguration configuration, ILogger<Vp9RendererService> logger,
            VideoSubjectService screenSubjectService, ScreenshotHelper screenshot
        ) : base(lifetime, logger, screenSubjectService)
        {
            _logger = logger;
            _screenshot = screenshot;
        }

        protected override void SnapshotScreen(Gba gba, VpxImage image)
        {
            Debug.Assert(image.DisplayedWidth == GbaHostService.GBA_WIDTH);
            Debug.Assert(image.DisplayedHeight == GbaHostService.GBA_HEIGHT);
            Debug.Assert(image.Format == vpx_img_fmt_t.VPX_IMG_FMT_I444);

            int colorMask = ScreenshotHelper.COLOR_MASK;
            byte[] yLut = _screenshot.YLut;
            byte[] uLut = _screenshot.ULut;
            byte[] vLut = _screenshot.VLut;

            Span<ushort> screen = gba.Ppu.Renderer.ScreenFront;
            for (int j = 0; j < GbaHostService.GBA_HEIGHT; j++)
            {
                int indexY = 0;
                int indexU = 0;
                int indexV = 0;
                Span<byte> rowY = image.GetRowY(j);
                Span<byte> rowU = image.GetRowU(j);
                Span<byte> rowV = image.GetRowV(j);

                for (int i = 0; i < GbaHostService.GBA_WIDTH; i++)
                {
                    int rgb555 = screen[i + j * GbaHostService.GBA_WIDTH] & colorMask;
                    byte y = yLut[rgb555];
                    byte u = uLut[rgb555];
                    byte v = vLut[rgb555];

                    rowY[indexY++] = y;
                    rowU[indexU++] = u;
                    rowV[indexV++] = v;
                }
            }
        }

        protected override VpxImage ProvideScreenBuffer()
        {
            return new VpxImage(VPX_IMG_FMT_I444, GBA_WIDTH, GBA_HEIGHT);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            using Vp9Encoder vp9 = new Vp9Encoder((ref vpx_codec_enc_cfg_t config) =>
            {
                config.g_w = GBA_WIDTH;
                config.g_h = GBA_HEIGHT;
                config.g_timebase.num = CYCLES_PER_FRAME;
                config.g_timebase.den = CYCLES_PER_SECONDS;
                config.g_lag_in_frames = 0; // Realtime output
                config.g_error_resilient = VPX_ERROR_RESILIENT_DEFAULT;
                config.g_threads = (uint)Environment.ProcessorCount;
                config.g_profile = 1; // Profile 1: YUV444 with 8 bit frames
            });
            vp9.Control(VP9E_SET_LOSSLESS, 1); // on
            vp9.Control(VP8E_SET_CPUUSED, -5); // [-9, 9]
            vp9.Control(VP9E_SET_TUNE_CONTENT, (int)VP9E_CONTENT_SCREEN);
            vp9.Control(VP9E_SET_COLOR_RANGE, 1); // full
            vp9.Control(VP9E_SET_SVC_INTER_LAYER_PRED, 1); // off all
            vp9.Control(VP9E_SET_DISABLE_LOOPFILTER, 2); // off all

            int bufferPoolSize = 16;
            int bufferSize = 0x20000; // 128k
            int bufferPoolIndex = 0;
            byte[] frameBuffer = new byte[bufferSize * bufferPoolSize];

            long frames = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                void ProcessFrame(VpxImage screenBuffer) {
                    vp9.Encode(screenBuffer, frames, 1, VPX_EFLAG_NONE, VPX_DL_GOOD_QUALITY);
                }
                await DequeueFrame(ProcessFrame, cancellationToken);

                foreach (VpxPacket packet in vp9.GetCXData())
                {
                    if (packet.Kind == VPX_CODEC_CX_FRAME_PKT)
                    {
                        int bufferOffset = bufferPoolIndex * bufferSize;
                        bufferPoolIndex = (bufferOffset + 1) % bufferPoolSize;
                        var frame = packet.DataAsFrame;
                        Memory<byte> buffer = new Memory<byte>(frameBuffer, bufferOffset, bufferSize);
                        GbaHostService.VIDEO_FRAME_HEADER.CopyTo(buffer.Span);
                        frame.Buf.CopyTo(buffer.Span.Slice(GbaHostService.FRAME_HEADER_LENGTH));
                        FlushFrame(new VideoSubjectPayload()
                        {
                            Buffer = buffer.Slice(0, frame.Buf.Length + GbaHostService.FRAME_HEADER_LENGTH),
                            FrameMetadata = new FrameMetadata()
                            {
                                IsKey = true//(frame.Flags & VPX_FRAME_IS_KEY) == VPX_FRAME_IS_KEY
                            }
                        });
                    }
                }
            }
        }

    }
}
