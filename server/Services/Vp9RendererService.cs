using System;
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
            ScreenSubjectService screenSubjectService, ScreenshotHelper screenshot
        ) : base(lifetime, logger, screenSubjectService)
        {
            _logger = logger;
            _screenshot = screenshot;
        }

        protected override void SnapshotScreen(Gba gba, VpxImage screenBuffer)
        {
            _screenshot.Take(gba, screenBuffer);
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
                        frame.Buf.CopyTo(new Span<byte>(frameBuffer, bufferOffset, bufferSize));
                        FlushFrame(new ScreenSubjectPayload()
                        {
                            Buffer = new ReadOnlyMemory<byte>(frameBuffer, bufferOffset, (int)frame.Buf.Length),
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
