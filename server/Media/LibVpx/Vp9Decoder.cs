using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vp8dx;

namespace OptimeGBAServer.Media.LibVpx
{
    public unsafe class Vp9Decoder : VpxDecoder
    {
        public Vp9Decoder() : base() {}
        public Vp9Decoder(Configurator configurator) : base(configurator) {}

        protected override vpx_codec_iface_t* GetIFace()
        {
            return vpx_codec_vp9_dx();
        }
    }
}
