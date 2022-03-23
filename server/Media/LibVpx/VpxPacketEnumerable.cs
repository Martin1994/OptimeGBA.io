using System;
using System.Collections;
using System.Collections.Generic;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_encoder;

namespace OptimeGBAServer.Media.LibVpx
{
    public unsafe struct VpxPacketEnumerable : IEnumerable<VpxPacket>
    {
        public struct Enumerator : IEnumerator<VpxPacket>
        {
            public VpxPacket Current { get; private set; }

            object IEnumerator.Current => Current;

            private readonly vpx_codec_ctx_t* _codec;
            private readonly vpx_codec_iter_t _iter;

            public Enumerator(vpx_codec_ctx_t* codec)
            {
                _codec = codec;
                _iter = default;
                Current = default;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                vpx_codec_cx_pkt_t* packet;
                fixed (vpx_codec_iter_t* iterRef = &_iter)
                {
                    packet = vpx_codec_get_cx_data(_codec, iterRef);
                }
                VpxCodec.ThrowIfNotOK(_codec);

                Current = new VpxPacket(packet);
                return packet != null;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        private readonly vpx_codec_ctx_t* _codec;

        public VpxPacketEnumerable(vpx_codec_ctx_t* codec)
        {
            _codec = codec;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_codec);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<VpxPacket> IEnumerable<VpxPacket>.GetEnumerator() => GetEnumerator();
    }
}
