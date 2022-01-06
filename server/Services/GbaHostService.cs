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
using OptimeGBAServer.Models;
using OptimeGBAServer.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace OptimeGBAServer
{
    public class GbaHostService : IHostedService
    {
        public const int GBA_WIDTH = 240;
        public const int GBA_HEIGHT = 160;
        private const int CYCLES_PER_FRAME_GBA = 280896;
        private const double SECONDS_PER_FRAME_GBA = 1D / (16777216D / 280896D);

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

            if (optimeConfig.BiosHome == null)
            {
                throw new InitializationException("GBA BIOS home is not provided.");
            }
            _gbaBiosHome = optimeConfig.BiosHome;

            if (optimeConfig.Rom == null)
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

            using PeriodicTimer mainClock = new PeriodicTimer(TimeSpan.FromSeconds(SECONDS_PER_FRAME_GBA));
            Stopwatch fpsStopwatch = new Stopwatch();

            _logger.LogInformation("GBA started.");

            long cyclesLeft = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                await mainClock.WaitForNextTickAsync(cancellationToken);

                if (_screenSubjectService.ObserverCount == 0)
                {
                    continue;
                }

                cyclesLeft += CYCLES_PER_FRAME_GBA;
                while (cyclesLeft > 0)
                {
                    cyclesLeft -= gba.StateStep();
                }

                if (gba.Ppu.Renderer.RenderingDone)
                {
                    _screenshot.Take(gba, screen);
                    int bufferOffset = bufferPoolIndex * bufferSize;
                    bufferPoolIndex = (bufferPoolIndex + 1) % bufferPoolSize;
                    using MemoryStream encoded = new MemoryStream(screenBuffer, bufferOffset, bufferSize);
                    screen.Save(encoded, encoder);
                    _screenSubjectService.BufferWriter.TryWrite(new ReadOnlyMemory<byte>(screenBuffer, bufferOffset, (int)encoded.Position));
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
            if (!System.IO.File.Exists(gbaBiosPath))
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

            return new Gba(provider);
        }
    }
}
