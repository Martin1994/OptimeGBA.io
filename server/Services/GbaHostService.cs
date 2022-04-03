using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimeGBA;
using OptimeGBAServer.Collections;
using OptimeGBAServer.Exceptions;
using OptimeGBAServer.Media;
using OptimeGBAServer.Models;


namespace OptimeGBAServer.Services
{
    public class GbaHostService : DaemonService
    {
        public const int GBA_WIDTH = 240;
        public const int GBA_HEIGHT = 160;
        public const int CYCLES_PER_FRAME = 280896;
        public const int CYCLES_PER_SECONDS = 0x1000000;
        private const double SECONDS_PER_FRAME = (double)CYCLES_PER_FRAME / (double)CYCLES_PER_SECONDS;

        public const int FRAME_HEADER_LENGTH = 4;
        public static readonly byte[] VIDEO_FRAME_HEADER = new byte[FRAME_HEADER_LENGTH] { 0, 0, 0, 0 };
        public static readonly byte[] AUDIO_FRAME_HEADER = new byte[FRAME_HEADER_LENGTH] { 1, 0, 0, 0 };

        private readonly ILogger _logger;

        private string _gbaBiosHome;
        private string _romPath;

        private readonly WindowAverage _fps = new WindowAverage(60);
        public double Fps { get => _fps.Average; }

        public Gba? Emulator { get; private set; }

        private readonly VideoSubjectService _videoSubjectService;
        private readonly AudioSubjectService _audioSubjectService;
        private readonly IGbaRenderer _renderer;

        public GbaHostService(
            IHostApplicationLifetime lifetime, IConfiguration configuration, ILogger<GbaHostService> logger,
            IGbaRenderer renderer, VideoSubjectService videoSubjectService, ScreenshotHelper screenshot,
            AudioSubjectService audioSubjectService
        ) : base(lifetime, logger)
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
            _videoSubjectService = videoSubjectService;
            _audioSubjectService = audioSubjectService;
            _renderer = renderer;
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            Gba gba = Emulator = await ProvideGba(cancellationToken);

            using PeriodicTimer mainClock = new PeriodicTimer(TimeSpan.FromSeconds(SECONDS_PER_FRAME));
            Stopwatch frameStopwatch = Stopwatch.StartNew();

            _logger.LogInformation("GBA started.");

            long cyclesLeft = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                await mainClock.WaitForNextTickAsync(cancellationToken);

                if (_videoSubjectService.ObserverCount == 0)
                {
                    continue;
                }

                cyclesLeft += CYCLES_PER_FRAME;
                while (cyclesLeft > 0)
                {
                    cyclesLeft -= gba.StateStep();
                }

                if (gba.Ppu.Renderer.RenderingDone)
                {
                    await _renderer.EnqueueFrame(gba, cancellationToken);
                }

                if (gba.Mem.SaveProvider.Dirty)
                {
                    gba.Mem.SaveProvider.Dirty = false;
                    try
                    {
                        _logger.LogInformation("Save dirty. Flusing to disk... {0}", gba.Provider.SavPath);
                        await File.WriteAllBytesAsync(gba.Provider.SavPath, gba.Mem.SaveProvider.GetSave(), cancellationToken);
                    }
                    catch
                    {
                        _logger.LogInformation("Failed to write sav file to {0}.", gba.Provider.SavPath);
                    }
                }

                _fps.AddSample(Math.Clamp(1 / frameStopwatch.Elapsed.TotalSeconds, 0, 999d));

                frameStopwatch.Restart();
            }
        }

        private byte[] _audioBuffer = new byte[0x200000]; // 2048k
        private int _audioBufferOffset = 0;
        private void FlushAudio(short[] stereo16BitInterleavedData)
        {
            Span<byte> source = MemoryMarshal.Cast<short, byte>(stereo16BitInterleavedData.AsSpan());

            if (_audioBufferOffset + source.Length >= _audioBuffer.Length) {
                _audioBufferOffset = 0;
            }
            Memory<byte> buffer = new Memory<byte>(_audioBuffer, _audioBufferOffset, source.Length + FRAME_HEADER_LENGTH);
            _audioBufferOffset += stereo16BitInterleavedData.Length;

            AUDIO_FRAME_HEADER.CopyTo(buffer.Span);
            source.CopyTo(buffer.Span.Slice(FRAME_HEADER_LENGTH));

            _audioSubjectService.BufferWriter.TryWrite(new AudioSubjectPayload()
            {
                Buffer = buffer
            });
        }

        private async Task<Gba> ProvideGba(CancellationToken cancellationToken)
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
                gbaBios = await File.ReadAllBytesAsync(gbaBiosPath, cancellationToken);
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
                rom = await File.ReadAllBytesAsync(_romPath, cancellationToken);
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
                    sav = await File.ReadAllBytesAsync(savPath, cancellationToken);
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

            var provider = new ProviderGba(gbaBios, rom, savPath, FlushAudio);
            provider.BootBios = true;

            Gba gba = new Gba(provider);
            gba.Mem.SaveProvider.LoadSave(sav);
            return gba;
        }

    }
}
