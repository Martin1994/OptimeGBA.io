using OptimeGBAServer.Media.LibX264.Native;
using System;
using System.Runtime.InteropServices;
using static OptimeGBAServer.Media.LibX264.Native.x264;

namespace OptimeGBAServer.Media.LibX264
{
    public unsafe class X264Picture : IDisposable
    {
        private readonly x264_picture_t* _picture;
        private bool disposedValue;

        public delegate void Configurator(ref x264_param_t config);
        private static Configurator _noopConfigurator = (ref x264_param_t config) => {};


        public X264Picture(X264Csp csp, int width, int height)
        {
            _picture = (x264_picture_t*)Marshal.AllocHGlobal(sizeof(x264_picture_t)).ToPointer();

            x264_picture_alloc(_picture, csp, width, height);
        }

        public x264_picture_t* Pointer => _picture;

        public X264Type Type => _picture->i_type;

        public int QPPlus1 => _picture->i_qpplus1;

        public int PicStruct => _picture->i_pic_struct;

        public bool KeyFrame => _picture->b_keyframe != 0;

        public long Pts => _picture->i_pts;

        public long Dts => _picture->i_dts;

        public x264_param_t* param => _picture->param;

        public x264_image_t img => _picture->img;

        public x264_image_properties_t prop => _picture->prop;

        public x264_hrd_t hrd_timing => _picture->hrd_timing;

        public x264_sei_t extra_sei => _picture->extra_sei;

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
                x264_picture_clean(_picture);
            }
        }

        ~X264Picture()
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
