using System;
using OptimeGBAServer.Media.LibOpenH264.Native;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public unsafe struct OpenH264LayerBSInfo
    {
        private SLayerBSInfo* _info;

        public OpenH264LayerBSInfo(SLayerBSInfo* info)
        {
            _info = info;
        }

        public byte TemporalId => _info->uiTemporalId;

        public byte SpatialId => _info->uiSpatialId;

        public byte QualityId => _info->uiQualityId;

        public EVideoFrameType FrameType => _info->eFrameType;

        public LAYER_TYPE LayerType => (LAYER_TYPE)_info->uiLayerType;

        public int SubSeqId => _info->iSubSeqId;

        public int NalCount => _info->iNalCount;
        public NalEnumerable Nal => new NalEnumerable(_info);

        public Span<byte> BsBuf
        {
            get
            {
                Span<int> nalLength = new Span<int>(_info->pNalLengthInByte, _info->iNalCount);
                int totalLength = 0;
                for (int i = 0; i < nalLength.Length; i++)
                {
                    totalLength += nalLength[i];
                }
                return new Span<byte>(_info->pBsBuf, totalLength);
            }
        }

        public unsafe struct NalEnumerable
        {
            private SLayerBSInfo* _info;

            public NalEnumerable(SLayerBSInfo* info)
            {
                _info = info;
            }

            public NalEnumerator GetEnumerator()
            {
                return new NalEnumerator(_info);
            }
        }

        public struct NalEnumerator
        {
            private SLayerBSInfo* _info;
            private int _index;
            private int _offset;

            public NalEnumerator(SLayerBSInfo* info)
            {
                _info = info;
                _index = default;
                _offset = default;
                Reset();
            }

            public Span<byte> Current => new Span<byte>(_info->pBsBuf + (_offset - _info->pNalLengthInByte[_index]), _info->pNalLengthInByte[_index]);

            public void Dispose() { }

            public bool MoveNext()
            {
                _index++;
                _offset += _info->pNalLengthInByte[_index];
                return _index < _info->iNalCount;
            }

            public void Reset()
            {
                _index = -1;
                _offset = 0;
            }
        }
    }
}
