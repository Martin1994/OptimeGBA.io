
using System;
using System.Runtime.InteropServices;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec;
using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_err_t;

namespace OptimeGBAServer.Media.LibVpx
{
    public static class VpxCodec
    {
        public static unsafe void ThrowIfNotOK(vpx_codec_ctx_t* codec)
        {
            if (codec->err != VPX_CODEC_OK)
            {
                throw new VpxException(Marshal.PtrToStringAnsi(new IntPtr(codec->err_detail)) ?? "Error without detail.", codec->err);
            }
        }
    }
    public abstract unsafe class VpxCodec<TConfig> : IDisposable where TConfig : unmanaged
    {
        protected readonly vpx_codec_ctx_t* _codec;
        protected readonly TConfig* _config;

        private bool disposedValue;

        public VpxCodec()
        {
            _codec = (vpx_codec_ctx_t*)Marshal.AllocHGlobal(sizeof(vpx_codec_ctx_t)).ToPointer();
            _codec->err_detail = null;
            _config = (TConfig*)Marshal.AllocHGlobal(sizeof(TConfig)).ToPointer();
        }

        public void Control(vp8e_enc_control_id controlId, int data)
        {
            vpx_codec_control_(_codec, controlId, __arglist(data));
            ThrowIfNotOK();
        }

        public void ThrowIfNotOK() => VpxCodec.ThrowIfNotOK(_codec);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                vpx_codec_destroy(_codec);
                Marshal.FreeHGlobal(new IntPtr(_codec));
                Marshal.FreeHGlobal(new IntPtr(_config));
                disposedValue = true;
            }
        }

        ~VpxCodec()
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
}
