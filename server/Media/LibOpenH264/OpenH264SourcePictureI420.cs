using static OptimeGBAServer.Media.LibOpenH264.Native.EVideoFormatType;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public unsafe class OpenH264SourcePictureI420 : OpenH264SourcePicture
    {
        public OpenH264SourcePictureI420(int width, int height) : base(width, height)
        {
            _picture->iColorFormat = videoFormatI420;
        }

        protected override int GetXChromaShift(int index)
        {
            switch (index)
            {
                case 0:
                    return 0;

                case 1:
                case 2:
                    return 1;

                default:
                    return 4096;
            }
        }

        protected override int GetYChromaShift(int index)
        {
            switch (index)
            {
                case 0:
                    return 0;

                case 1:
                case 2:
                    return 1;

                default:
                    return 4096;
            }
        }
    }
}
