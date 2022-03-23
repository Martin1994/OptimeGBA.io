using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OptimeGBA;
using OptimeGBAServer.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OptimeGBAServer.Media
{
    public class ScreenshotHelper
    {
        private const int COLOR_COUNT = 0x8000; // 15 bit color;
        public const int COLOR_MASK = 0x7FFF;
        private readonly Color[] _colorLut = new Color[COLOR_COUNT];
        private readonly Rgba32[] _rgbLut = new Rgba32[COLOR_COUNT];
        public readonly byte[] YLut = new byte[COLOR_COUNT];
        public readonly byte[] ULut = new byte[COLOR_COUNT];
        public readonly byte[] VLut = new byte[COLOR_COUNT];

        public ScreenshotHelper()
        {
            // Generate lut
            for (ushort i = 0; i < _colorLut.Length; i++)
            {
                _colorLut[i] = Rgb555ToRgba32(i);
                _rgbLut[i] = Rgb555ToRgba32(i);
                Rgba32 yuv = Rgb555ToYuv420(i);
                YLut[i] = yuv.R;
                ULut[i] = yuv.G;
                VLut[i] = yuv.B;
            }
        }

        public void Take(Gba gba, Image<Rgba32> image)
        {
            Debug.Assert(image.Width == GbaHostService.GBA_WIDTH);
            Debug.Assert(image.Height == GbaHostService.GBA_HEIGHT);
            Span<ushort> buffer = gba.Ppu.Renderer.ScreenFront;
            for (int y = 0; y < GbaHostService.GBA_HEIGHT; y++)
            {
                for (int x = 0; x < GbaHostService.GBA_WIDTH; x++)
                {
                    image[x, y] = _rgbLut[buffer[x + y * GbaHostService.GBA_WIDTH] & COLOR_MASK];
                }
            }
        }

        public void GetPalette(Gba gba, Span<Color> palette)
        {
            Debug.Assert(palette.Length == 0x200);
            Span<ushort> innerPalette = MemoryMarshal.Cast<byte, ushort>(gba.Ppu.Renderer.Palettes);
            for (int i = 0; i < 0x200; i++)
            {
                palette[i] = _colorLut[innerPalette[i] & COLOR_MASK];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba32 Rgb555ToRgba32(uint data)
        {
            double r = (data >> 0) & 0b11111;
            double g = (data >> 5) & 0b11111;
            double b = (data >> 10) & 0b11111;

            const double factor555To888 = 255.0d / 31.0d;
            byte fr = (byte)Math.Clamp(factor555To888 * r, 0, 255);
            byte fg = (byte)Math.Clamp(factor555To888 * g, 0, 255);
            byte fb = (byte)Math.Clamp(factor555To888 * b, 0, 255);

            return new Rgba32(fr, fg, fb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba32 Rgb555ToYuv420(uint data)
        {
            double r = ((data >> 0) & 0b11111);
            double g = ((data >> 5) & 0b11111);
            double b = ((data >> 10) & 0b11111);

            const double coef555to888 = 255.0d / 31.0d;
            double fr = coef555to888 * r;
            double fg = coef555to888 * g;
            double fb = coef555to888 * b;

            double y = (0.257 * fr + 0.504 * fg + 0.098 * fb) + 16;
            double u = (-0.148 * fr - 0.291 * fg + 0.439 * fb) + 128;
            double v = (0.439 * fr - 0.368 * fg - 0.071 * fb) + 128;

            return new Rgba32(
                (byte)Math.Clamp(y, 0, 255),
                (byte)Math.Clamp(u, 0, 255),
                (byte)Math.Clamp(v, 0, 255)
            );
        }
    }
}
