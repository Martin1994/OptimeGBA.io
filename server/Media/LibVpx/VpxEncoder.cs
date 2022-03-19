using System;
using System.Diagnostics;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_err_t;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_encoder;

namespace OptimeGBAServer.Media.LibVpx
{
    public abstract unsafe class VpxEncoder : VpxCodec<vpx_codec_enc_cfg_t>
    {
        public delegate void Configurator(ref vpx_codec_enc_cfg_t config);

        public VpxEncoder(Configurator configure) : base()
        {
            vpx_codec_iface_t* iface = GetIFace();
            vpx_codec_err_t res = vpx_codec_enc_config_default(iface, _config, 0);
            if (res != VPX_CODEC_OK)
            {
                throw new VpxException("Cannot assign defaults to config.", res);
            }

            configure(ref *_config);

            vpx_codec_enc_init(_codec, iface, _config, 0);
            ThrowIfNotOK();
        }

        protected abstract vpx_codec_iface_t* GetIFace();

        public void Encode(VpxImage image, vpx_codec_pts_t pts, uint duration, vpx_enc_frame_flags_t flags, vpx_enc_deadline_flags_t deadline)
        {
            vpx_codec_encode(_codec, image.Raw, pts, duration, flags, deadline);
            ThrowIfNotOK();
        }

        public VpxPacketEnumerable GetCXData()
        {
            return new VpxPacketEnumerable(_codec);
        }
    }
}
