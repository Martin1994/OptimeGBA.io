using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OptimeGBAServer.Media;
using OptimeGBAServer.Media.LibVpx.Native;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_encoder;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_image;

namespace OptimeGBAServer
{
    public class MainWeb
    {
        private static void InjectDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<GbaHostService>();
            builder.Services.AddHostedService<GbaHostService>(s => s.GetRequiredService<GbaHostService>());

            builder.Services.AddSingleton<ScreenSubjectService>();
            builder.Services.AddHostedService<ScreenSubjectService>(s => s.GetRequiredService<ScreenSubjectService>());

            builder.Services.AddSingleton<ScreenshotHelper>();

            builder.Services.AddControllers();
        }

        public static async Task Main(string[] args)
        {
            TestLibVpx();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            InjectDependencies(builder);

            WebApplication app = builder.Build();

            app.UseWebSockets();

            app.MapControllers();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            await app.RunAsync();
        }

        public unsafe static void TestLibVpx()
        {
            // A simplified version of https://github.com/webmproject/libvpx/blob/main/examples/vp9_lossless_encoder.c

            vpx_codec_ctx_t codec;
            vpx_codec_enc_cfg_t cfg;
            int frame_count = 0;
            vpx_image_t* raw;
            vpx_codec_err_t res;

            const int width = 240;
            const int height = 160;

            raw = vpx_img_alloc(null, vpx_img_fmt_t.VPX_IMG_FMT_I420, width, height, 1);
            if (raw is null)
            {
                throw new Exception("Failed to allocate image.");
            }

            vpx_codec_iface_t* iface = vp8cx.vpx_codec_vp9_cx();
            res = vpx_encoder.vpx_codec_enc_config_default(iface, &cfg, 0);
            if (res != vpx_codec_err_t.VPX_CODEC_OK) throw new Exception("Failed to get default codec config.");

            cfg.g_w = width;
            cfg.g_h = height;
            cfg.g_timebase.num = 280896;
            cfg.g_timebase.den = 16777216;
            cfg.g_lag_in_frames = 0;
            cfg.g_error_resilient = vpx_codec_er_flags_t.VPX_ERROR_RESILIENT_DEFAULT;

            res = vpx_codec_enc_init(&codec, iface, &cfg, 0);
            if (res != vpx_codec_err_t.VPX_CODEC_OK)
                throw new Exception("Failed to initialize encoder");

            res = vpx_codec_control_(&codec, vp8e_enc_control_id.VP9E_SET_LOSSLESS, __arglist(1));
            if (res != vpx_codec_err_t.VPX_CODEC_OK)
                throw new Exception("Failed to use lossless mode");

            for (int i = 0; i < 30; i++)
                EncodeFrame(&codec, raw, frame_count++, vpx_enc_frame_flags_t.VPX_EFLAG_NONE);

            // Flush encoder.
            EncodeFrame(&codec, null, -1, vpx_enc_frame_flags_t.VPX_EFLAG_NONE);

            vpx_img_free(raw);
            if (vpx_codec_destroy(&codec) != vpx_codec_err_t.VPX_CODEC_OK) throw new Exception("Failed to destroy codec.");
        }

        private static unsafe int EncodeFrame(vpx_codec_ctx_t *codec, vpx_image_t *img,
                        int frame_index, vpx_enc_frame_flags_t flags) {
            int got_pkts = 0;
            vpx_codec_iter_t iter;
            vpx_codec_cx_pkt_t *pkt = null;
            vpx_codec_err_t res =
                vpx_codec_encode(codec, img, frame_index, 1, flags, vpx_enc_deadline_flags_t.VPX_DL_REALTIME);
            if (res != vpx_codec_err_t.VPX_CODEC_OK) throw new Exception("Failed to encode frame");

            while ((pkt = vpx_codec_get_cx_data(codec, &iter)) != null) {
                got_pkts++;

                if (pkt->kind == vpx_codec_cx_pkt_kind.VPX_CODEC_CX_FRAME_PKT) {
                    bool keyframe = (pkt->data.frame.flags & vpx_codec_frame_flags_t.VPX_FRAME_IS_KEY) != 0;

                    Console.WriteLine(keyframe ? "K" : ".");
                }
            }

            return got_pkts;
        }
    }
}
