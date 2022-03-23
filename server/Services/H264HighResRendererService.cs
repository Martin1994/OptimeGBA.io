using OptimeGBA;
using OptimeGBAServer.Media;
using OptimeGBAServer.Media.LibOpenH264;
using OptimeGBAServer.Media.LibOpenH264.Native;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static OptimeGBAServer.Services.GbaHostService;
using static OptimeGBAServer.Media.LibOpenH264.Native.EVideoFormatType;
using static OptimeGBAServer.Media.LibOpenH264.Native.EUsageType;

namespace OptimeGBAServer.Services
{
    public class H264HighResRendererService : H264RendererService
    {
        public H264HighResRendererService(
            IHostApplicationLifetime lifetime, ILogger<H264HighResRendererService> logger,
            ScreenSubjectService screenSubjectService, ScreenshotHelper screenshot
        ) : base(lifetime, logger, screenSubjectService, screenshot) { }

        protected override void SnapshotScreen(Gba gba, OpenH264SourcePicture image)
        {
            Debug.Assert(image.PicWidth == GbaHostService.GBA_WIDTH * 2);
            Debug.Assert(image.PicHeight == GbaHostService.GBA_HEIGHT * 2);
            Debug.Assert(image.ColorFormat == videoFormatI420);

            int colorMask = ScreenshotHelper.COLOR_MASK;
            byte[] yLut = _screenshot.YLut;
            byte[] uLut = _screenshot.ULut;
            byte[] vLut = _screenshot.VLut;

            int yStride = image.Stride[0];
            int uStride = image.Stride[1];
            int vStride = image.Stride[2];

            Span<byte> yData = image.GetData(0);
            Span<byte> uData = image.GetData(1);
            Span<byte> vData = image.GetData(2);

            Span<ushort> screen = gba.Ppu.Renderer.ScreenFront;
            for (int j = 0; j < GbaHostService.GBA_HEIGHT; j++)
            {
                int indexY = 0;
                int indexU = 0;
                int indexV = 0;
                Span<byte> rowY = yData.Slice(j * 2 * yStride, yStride);
                Span<byte> rowYDup = yData.Slice((j * 2 + 1) * yStride, yStride);
                Span<byte> rowU = uData.Slice(j * uStride, uStride);
                Span<byte> rowV = vData.Slice(j * vStride, vStride);

                for (int i = 0; i < GbaHostService.GBA_WIDTH; i++)
                {
                    int rgb555 = screen[i + j * GbaHostService.GBA_WIDTH] & colorMask;

                    rowY[indexY] = yLut[rgb555];
                    rowY[indexY + 1] = yLut[rgb555];
                    rowYDup[indexY] = yLut[rgb555];
                    rowYDup[indexY + 1] = yLut[rgb555];
                    indexY += 2;

                    rowU[indexU++] = uLut[rgb555];
                    rowV[indexV++] = vLut[rgb555];
                }
            }
        }

        protected override OpenH264SourcePicture ProvideScreenBuffer()
        {
            return new OpenH264SourcePictureI420(GBA_WIDTH * 2, GBA_HEIGHT * 2);
        }

        protected override OpenH264Encoder CreateEncoder()
        {
            return new OpenH264Encoder((ref TagEncParamExt config) =>
            {
                config.iPicWidth = GBA_WIDTH * 2;
                config.iPicHeight = GBA_HEIGHT * 2;
                config.iUsageType = SCREEN_CONTENT_REAL_TIME;
                config.fMaxFrameRate = 0f;
                config.iTargetBitrate = 262144;
                config.iMultipleThreadIdc = 1; // Off
            });
        }
    }
}
