using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace OptimeGBAServer.Controllers
{
    [Route("/screen.{format:regex(bmp|gif|jpg|png|tga)}")]
    public class ScreenController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly GbaHostService _gba;
        private readonly ScreenshotHelper _screenshot;

        public ScreenController(ILogger<ScreenController> logger, GbaHostService gba, ScreenshotHelper screenshot)
        {
            _logger = logger;
            _gba = gba;
            _screenshot = screenshot;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string format, int quality = -1)
        {
            using Image<Rgba32> result = new Image<Rgba32>(GbaHostService.GBA_WIDTH, GbaHostService.GBA_HEIGHT);

            if (_gba.Emulator == null)
            {
                _logger.LogWarning("GBA emulater is not ready yet!");
            }
            else
            {
                _screenshot.Take(_gba.Emulator, result);
            }

            using MemoryStream encoded = new MemoryStream();
            await result.SaveAsync(encoded, GetEncoder(format, quality));

            return new FileContentResult(encoded.ToArray(), GetMime(format));
        }

        private static IImageEncoder GetEncoder(string format, int quality)
        {
            switch (format)
            {
                case "bmp":
                    return new BmpEncoder();

                case "gif":
                    if (quality == 0)
                    {
                        return new GifEncoder()
                        {
                            Quantizer = new WebSafePaletteQuantizer()
                        };
                    }
                    else if (quality == 1)
                    {
                        return new GifEncoder()
                        {
                            Quantizer = new WernerPaletteQuantizer()
                        };
                    }
                    else if (quality == 2)
                    {
                        return new GifEncoder()
                        {
                            Quantizer = new OctreeQuantizer()
                        };
                    }
                    else if (quality > 2)
                    {
                        return new GifEncoder()
                        {
                            Quantizer = new WuQuantizer()
                        };
                    }
                    else
                    {
                        return new GifEncoder();
                    }

                case "jpg":
                    if (quality >= 0)
                    {
                        return new JpegEncoder()
                        {
                            Quality = quality
                        };
                    }
                    else
                    {
                        return new JpegEncoder();
                    }

                case "tga":
                    return new TgaEncoder();

                default:
                    if (quality >= 0)
                    {
                        return new PngEncoder()
                        {
                            CompressionLevel = (PngCompressionLevel)Math.Clamp(quality, 0, 9)
                        };
                    }
                    else
                    {
                        return new PngEncoder();
                    }
            }
        }

        private static string GetMime(string format)
        {
            switch (format)
            {
                case "bmp":
                    return "image/bmp";

                case "gif":
                    return "image/gif";

                case "jpg":
                    return "image/jpeg";

                case "tga":
                    return "image/x-tga";

                default:
                    return "image/png";
            }
        }
    }
}
