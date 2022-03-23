using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OptimeGBAServer.Media.LibOpenH264.Native;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public unsafe class OpenH264FrameBSInfo : IDisposable
    {
        private bool _disposedValue;

        private SFrameBSInfo* _info;

        public OpenH264FrameBSInfo()
        {
            _info = (SFrameBSInfo*)Marshal.AllocHGlobal(sizeof(SFrameBSInfo)).ToPointer();
            Unsafe.InitBlockUnaligned(_info, 0, (uint)sizeof(SFrameBSInfo));
        }

        public SFrameBSInfo* Pointer => _info;

        public int LayerNum => _info->iLayerNum;

        public LayerEnumerable LayerInfo => new LayerEnumerable(_info);

        public EVideoFrameType FrameType => _info->eFrameType;

        public int FrameSizeInBytes => _info->iFrameSizeInBytes;

        public void CopyFrameData(Span<byte> destination)
        {
            destination = destination.Slice(0, FrameSizeInBytes);
            foreach (OpenH264LayerBSInfo layer in LayerInfo)
            {
                Span<byte> layerData = layer.BsBuf;
                layerData.CopyTo(destination);
                destination = destination.Slice(layerData.Length);
            }
            Debug.Assert(destination.Length == 0);
        }

        public long TimeStamp => _info->uiTimeStamp;

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
                Marshal.FreeHGlobal(new IntPtr(_info));
            }
        }

        ~OpenH264FrameBSInfo()
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

        public unsafe struct LayerEnumerable : IEnumerable<OpenH264LayerBSInfo>
        {
            private SFrameBSInfo* _info;

            public LayerEnumerable(SFrameBSInfo* info)
            {
                _info = info;
            }

            public LayerEnumerator GetEnumerator()
            {
                return new LayerEnumerator(_info);
            }

            IEnumerator<OpenH264LayerBSInfo> IEnumerable<OpenH264LayerBSInfo>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct LayerEnumerator : IEnumerator<OpenH264LayerBSInfo>
        {
            private SFrameBSInfo* _info;
            private int _index;

            public LayerEnumerator(SFrameBSInfo* info)
            {
                _info = info;
                _index = default;
                Reset();
            }

            public OpenH264LayerBSInfo Current => new OpenH264LayerBSInfo(&((SLayerBSInfo*)&_info->sLayerInfo)[_index]);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                _index++;
                return _index < _info->iLayerNum;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}
