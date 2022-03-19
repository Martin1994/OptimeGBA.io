// Generated by ClangSharp
// ClangSharpPInvokeGenerator --libraryPath=openh264 --file codec/api/svc/codec_def.h --namespace OptimeGBAServer.Media.LibOpenH264.Native --methodClassName openh264 --language c++ --config latest-codegen --output codec_def.cs

using System.Runtime.InteropServices;

namespace OptimeGBAServer.Media.LibOpenH264.Native
{
    public enum EVideoFormatType
    {
        videoFormatRGB = 1,
        videoFormatRGBA = 2,
        videoFormatRGB555 = 3,
        videoFormatRGB565 = 4,
        videoFormatBGR = 5,
        videoFormatBGRA = 6,
        videoFormatABGR = 7,
        videoFormatARGB = 8,
        videoFormatYUY2 = 20,
        videoFormatYVYU = 21,
        videoFormatUYVY = 22,
        videoFormatI420 = 23,
        videoFormatYV12 = 24,
        videoFormatInternal = 25,
        videoFormatNV12 = 26,
        videoFormatVFlip = unchecked((int)(0x80000000)),
    }

    public enum EVideoFrameType
    {
        videoFrameTypeInvalid,
        videoFrameTypeIDR,
        videoFrameTypeI,
        videoFrameTypeP,
        videoFrameTypeSkip,
        videoFrameTypeIPMixed,
    }

    public enum CM_RETURN
    {
        cmResultSuccess,
        cmInitParaError,
        cmUnknownReason,
        cmMallocMemeError,
        cmInitExpected,
        cmUnsupportedData,
    }

    public enum ENalUnitType
    {
        NAL_UNKNOWN = 0,
        NAL_SLICE = 1,
        NAL_SLICE_DPA = 2,
        NAL_SLICE_DPB = 3,
        NAL_SLICE_DPC = 4,
        NAL_SLICE_IDR = 5,
        NAL_SEI = 6,
        NAL_SPS = 7,
        NAL_PPS = 8,
    }

    public enum ENalPriority
    {
        NAL_PRIORITY_DISPOSABLE = 0,
        NAL_PRIORITY_LOW = 1,
        NAL_PRIORITY_HIGH = 2,
        NAL_PRIORITY_HIGHEST = 3,
    }

    public enum DeblockingIdcEnum
    {
        DEBLOCKING_IDC_0 = 0,
        DEBLOCKING_IDC_1 = 1,
        DEBLOCKING_IDC_2 = 2,
    }

    public enum ErrorToolEnum
    {
        ET_NONE = 0x00,
        ET_IP_SCALE = 0x01,
        ET_FMO = 0x02,
        ET_IR_R1 = 0x04,
        ET_IR_R2 = 0x08,
        ET_IR_R3 = 0x10,
        ET_FEC_HALF = 0x20,
        ET_FEC_FULL = 0x40,
        ET_RFS = 0x80,
    }

    public unsafe partial struct SliceInformation
    {
        [NativeTypeName("unsigned char *")]
        public byte* pBufferOfSlices;

        public int iCodedSliceCount;

        [NativeTypeName("unsigned int *")]
        public uint* pLengthOfSlices;

        public int iFecType;

        [NativeTypeName("unsigned char")]
        public byte uiSliceIdx;

        [NativeTypeName("unsigned char")]
        public byte uiSliceCount;

        [NativeTypeName("char")]
        public sbyte iFrameIndex;

        [NativeTypeName("unsigned char")]
        public byte uiNalRefIdc;

        [NativeTypeName("unsigned char")]
        public byte uiNalType;

        [NativeTypeName("unsigned char")]
        public byte uiContainingFinalNal;
    }

    public partial struct SRateThresholds
    {
        public int iWidth;

        public int iHeight;

        public int iThresholdOfInitRate;

        public int iThresholdOfMaxRate;

        public int iThresholdOfMinRate;

        public int iMinThresholdFrameRate;

        public int iSkipFrameRate;

        public int iSkipFrameStep;
    }

    public unsafe partial struct TagSysMemBuffer
    {
        public int iWidth;

        public int iHeight;

        public int iFormat;

        [NativeTypeName("int [2]")]
        public fixed int iStride[2];
    }

    public partial struct TagBufferInfo
    {
        public int iBufferStatus;

        [NativeTypeName("unsigned long long")]
        public ulong uiInBsTimeStamp;

        [NativeTypeName("unsigned long long")]
        public ulong uiOutYuvTimeStamp;

        [NativeTypeName("union (anonymous union at codec/api/svc/codec_def.h:201:3)")]
        public _UsrData_e__Union UsrData;

        [NativeTypeName("unsigned char *[3]")]
        public _pDst_e__FixedBuffer pDst;

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _UsrData_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("SSysMEMBuffer")]
            public TagSysMemBuffer sSystemBuffer;
        }

        public unsafe partial struct _pDst_e__FixedBuffer
        {
            public byte* e0;
            public byte* e1;
            public byte* e2;

            public ref byte* this[int index]
            {
                get
                {
                    fixed (byte** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    public static partial class openh264
    {
        [NativeTypeName("const char [6]")]
        public static readonly sbyte[] kiKeyNumMultiple = new sbyte[6]
        {
            1,
            1,
            2,
            4,
            8,
            16,
        };
    }
}
