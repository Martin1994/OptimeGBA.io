using System;
using OptimeGBAServer.Media.LibVpx.Native;

namespace OptimeGBAServer.Media.LibVpx
{
    public class VpxException : Exception
    {
        public vpx_codec_err_t Code { get; }

        public VpxException(string message): base(message) {}

        public VpxException(string message, vpx_codec_err_t code): base(message)
        {
            Code = code;
        }
    }
}
