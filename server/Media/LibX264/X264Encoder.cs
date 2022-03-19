using OptimeGBAServer.Media.LibX264.CheckError;
using OptimeGBAServer.Media.LibX264.Native;
using System;
using System.Runtime.InteropServices;
using static OptimeGBAServer.Media.LibX264.Native.x264;

namespace OptimeGBAServer.Media.LibX264
{
    public unsafe class X264Encoder : IDisposable
    {
        private readonly x264_t* _encoder;
        private bool disposedValue;

        private x264_picture_t* _pictureOut;
        public ref x264_picture_t PictureOut => ref *_pictureOut;

        public delegate void Configurator(ref x264_param_t config);
        private static Configurator _noopConfigurator = (ref x264_param_t config) => {};

        public X264Encoder(): this(X264Preset.None, X264Tune.None, _noopConfigurator) {}

        public X264Encoder(Configurator configurator): this(X264Preset.None, X264Tune.None, configurator) {}

        public X264Encoder(X264Preset preset, X264Tune tune): this(preset, tune, _noopConfigurator) {}

        public X264Encoder(X264Preset preset, X264Tune tune, Configurator configurator)
        {
            // x264 copies the param on open so we can safely allocate it on stack.
            x264_param_t param;
            fixed (sbyte* presetString = preset.ToCString(), tuneString = tune.ToCString())
            {
                x264_param_default_preset(&param, presetString, tuneString).X264CheckError();
            }

            configurator(ref param);

            _encoder = x264_encoder_open(&param);
            if (_encoder == null) {
                throw new X264Exception();
            }

            x264_param_cleanup(&param);

            _pictureOut = (x264_picture_t*)Marshal.AllocHGlobal(sizeof(x264_picture_t)).ToPointer();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                x264_encoder_close(_encoder);
                x264_picture_clean(_pictureOut);
            }
        }

        ~X264Encoder()
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
