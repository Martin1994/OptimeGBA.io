using System;
using OptimeGBAServer.Media.LibOpenH264.Assertion;
using OptimeGBAServer.Media.LibOpenH264.Native;
using static OptimeGBAServer.Media.LibOpenH264.Native.openh264;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public unsafe class OpenH264Encoder : IDisposable
    {
        private bool _disposedValue = false;
        private bool _initialized = false;

        private readonly ISVCEncoder* _encoder;

        public delegate void Configurator(ref TagEncParamExt config);

        public OpenH264Encoder(Configurator configure)
        {
            fixed (ISVCEncoder** encoderPtr = &_encoder)
            {
                WelsCreateSVCEncoder(encoderPtr).OpenH264AssertError();
                if (encoderPtr == null)
                {
                    throw new OpenH264Exception(CM_RETURN.cmMallocMemeError);
                }

                TagEncParamExt config;
                _encoder->GetDefaultParams(&config);
                configure(ref config);

                _encoder->InitializeExt(&config);
                _initialized = true;
            }
        }

        public void GetOption<T>(ENCODER_OPTION optionId, ref T option) where T : unmanaged
        {
            fixed (T* optionPtr = &option)
            {
                _encoder->GetOption(optionId, optionPtr).OpenH264AssertError();
            }
        }

        public void SetOption<T>(ENCODER_OPTION optionId, ref T option) where T : unmanaged
        {
            fixed (T* optionPtr = &option)
            {
                _encoder->SetOption(optionId, optionPtr).OpenH264AssertError();
            }
        }

        public void EncodeFrame(ref SSourcePicture sourcePicture, ref SFrameBSInfo info)
        {
            fixed (SFrameBSInfo* infoPtr = &info)
            fixed (SSourcePicture* sourcePicturePtr = &sourcePicture)
            {
                _encoder->EncodeFrame(sourcePicturePtr, infoPtr).OpenH264AssertError();
            }
        }

        public void EncodeParameterSets(ref SFrameBSInfo info)
        {
            fixed (SFrameBSInfo* infoPtr = &info)
            {
                _encoder->EncodeParameterSets(infoPtr).OpenH264AssertError();
            }
        }

        public void ForceIntraFrame(bool idr)
        {
            _encoder->ForceIntraFrame(idr ? (byte)0 : (byte)1).OpenH264AssertError();
        }

        public void ForceIntraFrame(bool idr, int layerId)
        {
            _encoder->ForceIntraFrame(idr ? (byte)0 : (byte)1, layerId).OpenH264AssertError();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;

                if (disposing)
                {
                    // Dispose managed state (managed objects)
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                if (_initialized)
                {
                    _encoder->Uninitialize();
                }
                WelsDestroySVCEncoder(_encoder);
            }
        }

        ~OpenH264Encoder()
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
