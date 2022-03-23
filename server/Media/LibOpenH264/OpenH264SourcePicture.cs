using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OptimeGBAServer.Media.LibOpenH264.Native;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public abstract unsafe class OpenH264SourcePicture : IDisposable
    {
        private bool _disposedValue = false;

        protected readonly SSourcePicture* _picture;
        protected readonly byte* _buffer;

        protected OpenH264SourcePicture(int bufferSize)
        {
            _picture = (SSourcePicture*)Marshal.AllocHGlobal(sizeof(SSourcePicture)).ToPointer();
            Unsafe.InitBlockUnaligned(_picture, 0, (uint)sizeof(SSourcePicture));

            _buffer = (byte*)Marshal.AllocHGlobal(sizeof(byte) * bufferSize).ToPointer();
            Unsafe.InitBlockUnaligned(_buffer, 0, (uint)(sizeof(byte) * bufferSize));

            _picture->uiTimeStamp = 0;
        }

        public SSourcePicture* Pointer => _picture;

        public EVideoFormatType ColorFormat => _picture->iColorFormat;

        private const int NUM_OF_PLANES = 4;

        public ReadOnlySpan<int> Stride => new Span<int>(_picture->iStride, NUM_OF_PLANES);

        public Span<byte> GetData(int index)
        {
            if (index < 0 || index >= NUM_OF_PLANES) {
                throw new IndexOutOfRangeException();
            }

            return new Span<byte>(_picture->pData[index], GetDataLength(index));
        }

        protected abstract int GetDataLength(int index);

        public int PicWidth => _picture->iPicWidth;

        public int PicHeight => _picture->iPicHeight;

        public long TimeStamp { get => _picture->uiTimeStamp; set => _picture->uiTimeStamp = value; }

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
                Marshal.FreeHGlobal(new IntPtr(_picture));
                Marshal.FreeHGlobal(new IntPtr(_buffer));
            }
        }

        ~OpenH264SourcePicture()
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
