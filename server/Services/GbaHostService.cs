using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimeGBA;
using OptimeGBAServer.Collections.Generics;
using OptimeGBAServer.Exceptions;
using OptimeGBAServer.Media;
using OptimeGBAServer.Media.LibVpx;
using OptimeGBAServer.Media.LibVpx.Native;
using OptimeGBAServer.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

using static OptimeGBAServer.Media.LibVpx.Native.vp8e_enc_control_id;
using static OptimeGBAServer.Media.LibVpx.Native.vp9e_tune_content;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_cx_pkt_kind;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_er_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_enc_deadline_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_enc_frame_flags_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_img_fmt_t;

namespace OptimeGBAServer
{
    public class GbaHostService : IHostedService
    {
        public const int GBA_WIDTH = 240;
        public const int GBA_HEIGHT = 160;
        private const int CYCLES_PER_FRAME = 280896;
        private const int CYCLES_PER_SECONDS = 0x1000000;
        private const double SECONDS_PER_FRAME = (double)CYCLES_PER_FRAME / (double)CYCLES_PER_SECONDS;

        private readonly ILogger _logger;

        private Task? _backgroundTask;
        private CancellationTokenSource? _backgroundCancellation;

        private string _gbaBiosHome;
        private string _romPath;

        private const int FPS_SAMPLE_SIZE = 60;
        private readonly RingBuffer<double> _fpsPool = new RingBuffer<double>(FPS_SAMPLE_SIZE);
        public double Fps { get; private set; }
        public Gba? Emulator { get; private set; }

        private readonly ScreenSubjectService _screenSubjectService;
        private readonly ScreenshotHelper _screenshot;

        public GbaHostService(IConfiguration configuration, ILogger<GbaHostService> logger, ScreenSubjectService screenSubjectService, ScreenshotHelper screenshot)
        {
            OptimeConfig optimeConfig = configuration.GetSection("Optime").Get<OptimeConfig>();

            if (optimeConfig.BiosHome is null)
            {
                throw new InitializationException("GBA BIOS home is not provided.");
            }
            _gbaBiosHome = optimeConfig.BiosHome;

            if (optimeConfig.Rom is null)
            {
                throw new InitializationException("ROM file is not provided.");
            }
            _romPath = optimeConfig.Rom;

            _logger = logger;
            _screenSubjectService = screenSubjectService;
            _screenshot = screenshot;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting up...");

            Emulator = await ProvideGba();

            _backgroundCancellation = new CancellationTokenSource();
            _backgroundTask =  Task.Factory.StartNew(async () => await RunAsync(Emulator, _backgroundCancellation.Token), creationOptions: TaskCreationOptions.LongRunning);

            await Task.CompletedTask;
        }

        private async Task RunAsync(Gba gba, CancellationToken cancellationToken)
        {
            IImageEncoder encoder = new PngEncoder()
            {
                FilterMethod = PngFilterMethod.None,
                CompressionLevel = PngCompressionLevel.BestSpeed,
                InterlaceMethod = PngInterlaceMode.None,
                TransparentColorMode = PngTransparentColorMode.Clear
            };

            // Allocate screen buffer
            using Image<Rgba32> screen = new Image<Rgba32>(GBA_WIDTH, GBA_HEIGHT);
            int bufferPoolSize = 16;
            int bufferSize = 0x20000; // 128k
            int bufferPoolIndex = 0;
            byte[] screenBuffer = new byte[bufferSize * bufferPoolSize];

            long frames = 0;
            using VpxImage image = new VpxImage(VPX_IMG_FMT_I420, GBA_WIDTH, GBA_HEIGHT);
            using Vp9Encoder vp9 = new Vp9Encoder((ref vpx_codec_enc_cfg_t config) =>
            {
                config.g_w = GBA_WIDTH;
                config.g_h = GBA_HEIGHT;
                config.g_timebase.num = CYCLES_PER_FRAME;
                config.g_timebase.den = CYCLES_PER_SECONDS;
                config.g_lag_in_frames = 0; // Realtime output
                config.g_error_resilient = VPX_ERROR_RESILIENT_DEFAULT;
                config.g_threads = 8;
            });
            vp9.Control(VP9E_SET_LOSSLESS, 1); // on
            vp9.Control(VP9E_SET_TUNE_CONTENT, (int)VP9E_CONTENT_SCREEN);
            vp9.Control(VP9E_SET_COLOR_RANGE, 1); // full
            //vp9.Control(VP9E_SET_SVC_INTER_LAYER_PRED, 1); // off all
            //vp9.Control(VP9E_SET_DISABLE_LOOPFILTER, 2); // off all
            using Vp9Decoder decoder = new Vp9Decoder();

            using PeriodicTimer mainClock = new PeriodicTimer(TimeSpan.FromSeconds(SECONDS_PER_FRAME));
            Stopwatch fpsStopwatch = new Stopwatch();

            _logger.LogInformation("GBA started.");

            long cyclesLeft = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                await mainClock.WaitForNextTickAsync(cancellationToken);

                if (_screenSubjectService.ObserverCount == 0)
                {
                    //continue;
                }

                cyclesLeft += CYCLES_PER_FRAME;
                while (cyclesLeft > 0)
                {
                    cyclesLeft -= gba.StateStep();
                }

                if (gba.Ppu.Renderer.RenderingDone)
                {
                    // VP9 loopback
                    _screenshot.Take(gba, image);
                    vp9.Encode(image, frames, 1, VPX_EFLAG_NONE, VPX_DL_REALTIME);
                    bool dirty = false;
                    foreach (VpxPacket packet in vp9.GetCXData())
                    {
                        if (packet.Kind == VPX_CODEC_CX_FRAME_PKT)
                        {
                            decoder.Decode(packet.DataAsFrame.Buf, IntPtr.Zero, 1);
                            foreach (VpxImage decoded in decoder.GetFrame())
                            {
                                Yuv420ToRgb(decoded, screen);
                                dirty = true;
                            }
                        }
                    }
                    if (dirty) {
                        // _screenshot.Take(gba, screen);
                        int bufferOffset = bufferPoolIndex * bufferSize;
                        bufferPoolIndex = (bufferPoolIndex + 1) % bufferPoolSize;
                        using MemoryStream encoded = new MemoryStream(screenBuffer, bufferOffset, bufferSize);
                        screen.Save(encoded, encoder);
                        _screenSubjectService.BufferWriter.TryWrite(new ReadOnlyMemory<byte>(screenBuffer, bufferOffset, (int)encoded.Position));
                    }
                }

                if (gba.Mem.SaveProvider.Dirty)
                {
                    gba.Mem.SaveProvider.Dirty = false;
                    try
                    {
                        _logger.LogInformation("Save dirty. Flusing to disk... {0}", gba.Provider.SavPath);
                        await File.WriteAllBytesAsync(gba.Provider.SavPath, gba.Mem.SaveProvider.GetSave());
                    }
                    catch
                    {
                        _logger.LogInformation("Failed to write sav file to {0}.", gba.Provider.SavPath);
                    }
                }

                double fps = Math.Clamp(1 / fpsStopwatch.Elapsed.TotalSeconds / (double)FPS_SAMPLE_SIZE, 0, 999d);

                if (_fpsPool.PushAndPopWhenFull(fps, out double poppedFps))
                {
                    Fps -= poppedFps;
                }
                Fps += fps;
                fpsStopwatch.Restart();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tearing down...");

            _backgroundCancellation?.Cancel();
            await (_backgroundTask ?? Task.CompletedTask);
        }

        private async Task<Gba> ProvideGba()
        {
            _logger.LogInformation("Booting GBA...");

            string gbaBiosPath = Path.Join(_gbaBiosHome, "gba_bios.bin");
            if (!File.Exists(gbaBiosPath))
            {
                throw new InitializationException("Please place a valid GBA BIOS under BIOS home named \"gba_bios.bin\"");
            }

            byte[] gbaBios;
            try
            {
                gbaBios = await File.ReadAllBytesAsync(gbaBiosPath);
            }
            catch (Exception ex)
            {
                throw new InitializationException("A GBA BIOS was provided, but there was an issue loading it.", ex);
            }

            if (!File.Exists(_romPath))
            {
                throw new InitializationException("The ROM file you provided does not exist.");
            }

            byte[] rom;
            try
            {
                rom = await File.ReadAllBytesAsync(_romPath);
            }
            catch (Exception ex)
            {
                throw new InitializationException("The ROM file you provided exists, but there was an issue loading it.", ex);
            }

            string savPath = _romPath.Substring(0, _romPath.Length - 3) + "sav";
            byte[] sav = new byte[0];

            if (File.Exists(savPath))
            {
                _logger.LogInformation(".sav exists, loading");
                try
                {
                    sav = await File.ReadAllBytesAsync(savPath);
                }
                catch (Exception ex)
                {
                    throw new InitializationException("Failed to load .sav file!", ex);
                }
            }
            else
            {
                _logger.LogWarning(".sav not available");
            }

            _logger.LogInformation("Loading GBA file");

            var provider = new ProviderGba(gbaBios, rom, savPath, x => {});
            provider.BootBios = true;

            Gba gba = new Gba(provider);
            gba.Mem.SaveProvider.LoadSave(sav);
            return gba;
        }

        private void Yuv420ToRgb(VpxImage from, Image<Rgba32> to)
        {
            Debug.Assert(from.Format == VPX_IMG_FMT_I420);
            int index = 0;

            for (int j = 0; j < GbaHostService.GBA_HEIGHT; j++)
            {
                int indexY = 0;
                int indexU = 0;
                int indexV = 0;
                Span<byte> rowY = from.GetRowY(j);
                Span<byte> rowU = from.GetRowU(j);
                Span<byte> rowV = from.GetRowV(j);
                for (int i = 0; i < GbaHostService.GBA_WIDTH; i++)
                {
                    double c = rowY[indexY++] - 16;
                    double d = rowU[indexU] - 128;
                    double e = rowV[indexV] - 128;

                    to[i, j] = new Rgba32(
                        (byte)Math.Clamp(1.164 * c +     0 * d + 1.596 * e, 0, 255),
                        (byte)Math.Clamp(1.164 * c - 0.391 * d - 0.813 * e, 0, 255),
                        (byte)Math.Clamp(1.164 * c + 2.018 * d +     0 * e, 0, 255)
                    );

                    if (index % 2 == 1)
                    {
                        indexU++;
                        indexV++;
                    }
                    index++;
                }
            }
        }
    }
}
