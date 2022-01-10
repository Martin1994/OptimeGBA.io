using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vp8cx;

namespace OptimeGBAServer.Media.LibVpx
{
    public unsafe class Vp9Encoder : VpxEncoder
    {
        public Vp9Encoder(Configurator configurator) : base(configurator) {}

        protected override vpx_codec_iface_t* GetIFace()
        {
            return vpx_codec_vp9_cx();
        }
    }
}
