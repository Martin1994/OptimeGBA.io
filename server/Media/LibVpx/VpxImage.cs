using System;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_image;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_img_fmt_t;

namespace OptimeGBAServer.Media.LibVpx
{
    public unsafe struct VpxImage : IDisposable
    {
        private class ImageContainer : IDisposable
        {
            private bool _disposedValue;
            public vpx_image_t* Raw;

            public ImageContainer(vpx_img_fmt_t format, uint width, uint height)
            {
                Raw = vpx_img_alloc(null, format, width, height, 1);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    vpx_img_free(Raw);
                    _disposedValue = true;
                }
            }

            ~ImageContainer()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
        public vpx_image_t* Raw { get; }
        private readonly ImageContainer? _imageContainer;

        public vpx_img_fmt_t Format => Raw->fmt;
        public vpx_color_space_t ColorSpace => Raw->cs;
        public vpx_color_range_t ColorRange => Raw->range;

        public uint Width => Raw->w;
        public uint Height => Raw->h;

        public uint Bitdepth => Raw->bit_depth;

        public uint DisplayedWidth => Raw->d_w;
        public uint DisplayedHeight => Raw->d_h;

        public uint RenderingWidth => Raw->r_w;
        public uint RenderingHeight => Raw->r_h;

        public uint XChromaShift => Raw->x_chroma_shift;
        public uint YChromaShift => Raw->y_chroma_shift;

        public Span<byte> PlanePacked => PlaneY;
        public Span<byte> PlaneY => new Span<byte>(Raw->plane_y, Raw->stride_y * (int)Raw->h);
        public Span<byte> PlaneU => new Span<byte>(Raw->plane_u, Raw->stride_u * (int)Raw->h);
        public Span<byte> PlaneV => new Span<byte>(Raw->plane_v, Raw->stride_v * (int)Raw->h);
        public Span<byte> PlaneAlpha => new Span<byte>(Raw->plane_alpha, Raw->stride_alpha * (int)Raw->h);

        public Span<byte> GetRowPacked(int index) => GetRowY(index);
        public Span<byte> GetRowY(int index) => new Span<byte>(Raw->plane_y + Raw->stride_y * index, (int)Raw->d_w * BytesPerDatum);
        public Span<byte> GetRowU(int index) => new Span<byte>(Raw->plane_u + Raw->stride_u * (index >> (int)Raw->y_chroma_shift), ((int)Raw->d_w >> (int)Raw->x_chroma_shift) * BytesPerDatum);
        public Span<byte> GetRowV(int index) => new Span<byte>(Raw->plane_v + Raw->stride_v * (index >> (int)Raw->y_chroma_shift), ((int)Raw->d_w >> (int)Raw->x_chroma_shift) * BytesPerDatum);
        public Span<byte> GetRowAlpha(int index) => new Span<byte>(Raw->plane_alpha + Raw->stride_alpha * index, (int)Raw->d_w * BytesPerDatum);

        public int StridePacked => Raw->stride_y;
        public int StrideY => Raw->stride_y;
        public int StrideU => Raw->stride_u;
        public int StrideV => Raw->stride_v;
        public int StrideAlpha => Raw->stride_alpha;

        public int BitsPerSample => Raw->bps;
        public int BytesPerDatum =>  (Raw->fmt & VPX_IMG_FMT_HIGHBITDEPTH) == 0 ? 1 : 2;

        public VpxImage(vpx_img_fmt_t format, uint width, uint height)
        {
            _imageContainer = new ImageContainer(format, width, height);
            Raw = _imageContainer.Raw;
        }

        public VpxImage(vpx_image_t* image)
        {
            _imageContainer = null;
            Raw = image;
        }

        public void Dispose()
        {
            _imageContainer?.Dispose();
        }
    }
}
