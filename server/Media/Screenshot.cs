using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OptimeGBA;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OptimeGBAServer.Media
{
    public class ScreenshotHelper
    {
        private const int COLOR_COUNT = 0x8000; // 15 bit color;
        private const int COLOR_MASK = 0x7FFF;
        private readonly Color[] _colorLut = new Color[COLOR_COUNT];
        private readonly Rgba32[] _rgbLut = new Rgba32[COLOR_COUNT];

        public ScreenshotHelper()
        {
            // Generate lut
            for (ushort i = 0; i < _colorLut.Length; i++)
            {
                _colorLut[i] = Rgb555ToRgba32(i);
                _rgbLut[i] = Rgb555ToRgba32(i);
            }
        }

        public void Take(Gba gba, Image<Rgba32> image)
        {
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
            byte fr = (byte)(factor555To888 * r);
            byte fg = (byte)(factor555To888 * g);
            byte fb = (byte)(factor555To888 * b);

            return new Rgba32(fr, fg, fb);
        }
    }
}
