using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OptimeGBAServer.Media.LibOpenH264.Native;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public abstract unsafe class OpenH264SourcePicture : IDisposable
    {
        private bool _disposedValue = false;

        protected readonly SSourcePicture* _picture = null;
        protected readonly byte* _buffer = null;

        private const int NUM_OF_PLANES = 4;

        protected OpenH264SourcePicture(int width, int height)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);

            _picture = (SSourcePicture*)Marshal.AllocHGlobal(sizeof(SSourcePicture)).ToPointer();
            Unsafe.InitBlockUnaligned(_picture, 0, (uint)sizeof(SSourcePicture));

            _picture->iPicWidth = width;
            _picture->iPicHeight = height;

            int bufferSize = 0;
            int bufferOffset = 0;
            Span<int> bufferOffsetOfPlane = stackalloc int[NUM_OF_PLANES];
            for (int i = 0; i < NUM_OF_PLANES; i++)
            {
                int xShift = GetXChromaShift(i);
                int yShift = GetYChromaShift(i);
                int offset = (width >> xShift) * (height >> yShift);

                _picture->iStride[i] = width >> xShift;
                bufferOffsetOfPlane[i] = bufferOffset;
                bufferOffset += offset;
                bufferSize += offset;
            }

            _buffer = (byte*)Marshal.AllocHGlobal(sizeof(byte) * bufferSize).ToPointer();
            Unsafe.InitBlockUnaligned(_buffer, 0, (uint)(sizeof(byte) * bufferSize));

            for (int i = 0; i < NUM_OF_PLANES; i++)
            {
                _picture->pData[i] = _buffer + bufferOffsetOfPlane[i];
            }

            _picture->uiTimeStamp = 0;
        }

        public SSourcePicture* Pointer => _picture;

        public EVideoFormatType ColorFormat => _picture->iColorFormat;

        public ReadOnlySpan<int> Stride => new Span<int>(_picture->iStride, NUM_OF_PLANES);

        public Span<byte> GetData(int index)
        {
            if (index < 0 || index >= NUM_OF_PLANES) {
                throw new IndexOutOfRangeException();
            }

            return new Span<byte>(_picture->pData[index], GetDataLength(index));
        }

        private int GetDataLength(int index)
        {
            int xShift = GetXChromaShift(index);
            int yShift = GetYChromaShift(index);
            return (PicWidth >> xShift) * (PicHeight >> yShift);
        }

        protected abstract int GetXChromaShift(int index);
        protected abstract int GetYChromaShift(int index);

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
