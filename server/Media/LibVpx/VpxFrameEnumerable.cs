using System;
using System.Collections;
using System.Collections.Generic;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_decoder;

namespace OptimeGBAServer.Media.LibVpx
{
    public unsafe struct VpxFrameEnumerable : IEnumerable<VpxImage>
    {
        public struct Enumerator : IEnumerator<VpxImage>
        {
            public VpxImage Current { get; private set; }

            object IEnumerator.Current => Current;

            private readonly vpx_codec_ctx_t* _codec;
            private readonly vpx_codec_iter_t _iter;

            public Enumerator(vpx_codec_ctx_t* codec)
            {
                _codec = codec;
                _iter = default;
                Current = default;
            }

            public void Dispose() {}

            public bool MoveNext()
            {
                vpx_image_t* image;
                fixed (vpx_codec_iter_t* iterRef = &_iter)
                {
                    image = vpx_codec_get_frame(_codec, iterRef);
                }
                VpxCodec.ThrowIfNotOK(_codec);

                Current = new VpxImage(image);
                return image != null;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        private readonly vpx_codec_ctx_t* _codec;

        public VpxFrameEnumerable(vpx_codec_ctx_t* codec)
        {
            _codec = codec;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_codec);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<VpxImage> IEnumerable<VpxImage>.GetEnumerator() => GetEnumerator();
    }
}
