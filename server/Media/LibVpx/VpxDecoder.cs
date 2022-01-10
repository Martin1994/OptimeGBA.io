using System;
using System.Runtime.CompilerServices;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_decoder;

namespace OptimeGBAServer.Media.LibVpx
{
    public abstract unsafe class VpxDecoder : VpxCodec<vpx_codec_dec_cfg_t>
    {
        public delegate void Configurator(ref vpx_codec_dec_cfg_t config);

        public VpxDecoder() : base()
        {
            vpx_codec_iface_t* iface = GetIFace();

            vpx_codec_dec_init(_codec, iface, null, 0);
            ThrowIfNotOK();
        }

        public VpxDecoder(Configurator configurator) : base()
        {
            vpx_codec_iface_t* iface = GetIFace();
            Unsafe.InitBlockUnaligned(_config, 0, (uint)sizeof(vpx_codec_dec_cfg_t));

            configurator(ref *_config);

            vpx_codec_dec_init(_codec, iface, _config, 0);
            ThrowIfNotOK();
        }

        protected abstract vpx_codec_iface_t* GetIFace();

        public void Decode(Span<byte> data, IntPtr userPriv, int deadline)
        {
            fixed (byte* buffer = data) {
                vpx_codec_decode(_codec, buffer, (uint)data.Length, userPriv.ToPointer(), deadline);
            }
            ThrowIfNotOK();
        }

        public VpxFrameEnumerable GetFrame()
        {
            return new VpxFrameEnumerable(_codec);
        }
    }
}
