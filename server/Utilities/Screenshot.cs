using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OptimeGBA;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OptimeGBAServer.Utilities
{
    public class ScreenshotHelper
    {
        private const int COLOR_COUNT = 0x8000; // 15 bit color;
        private const int COLOR_MASK = 0x7FFF;
        private readonly Color[] _colorLut = new Color[COLOR_COUNT];
        private readonly Rgba32[] _rgbLut = new Rgba32[COLOR_COUNT];
        private readonly byte[] _yLut = new byte[COLOR_COUNT];
        private readonly byte[] _uLut = new byte[COLOR_COUNT];
        private readonly byte[] _vLut = new byte[COLOR_COUNT];

        public ScreenshotHelper()
        {
            // Generate lut
            for (ushort i = 0; i < _colorLut.Length; i++)
            {
                _colorLut[i] = Rgb555ToRgba32(i);
                _rgbLut[i] = Rgb555ToRgba32(i);
                Rgba32 yuv = Rgb555ToYuv420(i);
                _yLut[i] = yuv.R;
                _uLut[i] = yuv.G;
                _vLut[i] = yuv.B;
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

        public void TakeYuv420(Gba gba, int width, int height, Span<byte> buffer)
        {
            Debug.Assert(buffer.Length == width * height * 3 / 2);

            int frameSize = width * height;
            int yIndex = 0;
            int vIndex = frameSize;
            int uIndex = frameSize + (frameSize / 4);
            int index = 0;

            Span<ushort> screen = gba.Ppu.Renderer.ScreenFront;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    int rgb555 = screen[i + j * GbaHostService.GBA_WIDTH] & COLOR_MASK;
                    byte y = _yLut[rgb555];
                    byte u = _uLut[rgb555];
                    byte v = _vLut[rgb555];

                    buffer[yIndex++] = y;

                    if (j % 2 == 0 && index % 2 == 0)
                    {
                        buffer[uIndex++] = u;
                        buffer[vIndex++] = v;
                    }

                    index++;
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
            double r = ((data >> 0) & 0b11111);
            double g = ((data >> 5) & 0b11111);
            double b = ((data >> 10) & 0b11111);

            const double coef555to888 = 255.0d / 31.0d;
            byte fr = (byte)(coef555to888 * r);
            byte fg = (byte)(coef555to888 * g);
            byte fb = (byte)(coef555to888 * b);

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

            int y = (int)(0.257 * fr + 0.504 * fg + 0.098 * fb) + 16;
            int u = (int)(0.439 * fr - 0.368 * fg - 0.071 * fb) + 128;
            int v = (int)(-0.148 * fr - 0.291 * fg + 0.439 * fb) + 128;

            return new Rgba32(
                (byte)((y < 0) ? 0 : ((y > 255) ? 255 : y)),
                (byte)((u < 0) ? 0 : ((u > 255) ? 255 : u)),
                (byte)((v < 0) ? 0 : ((v > 255) ? 255 : v))
            );
        }
    }
}
