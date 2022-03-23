using static OptimeGBAServer.Media.LibOpenH264.Native.EVideoFormatType;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public unsafe class OpenH264SourcePictureI420 : OpenH264SourcePicture
    {
        public OpenH264SourcePictureI420(int width, int height) : base(width * height * 3 / 2)
        {
            _picture->iPicWidth = width;
            _picture->iPicHeight = height;
            _picture->iColorFormat = videoFormatI420;

            _picture->iStride[0] = width;
            _picture->iStride[1] = width >> 1;
            _picture->iStride[2] = width >> 1;
            _picture->iStride[3] = 0;

            byte* dataStart = _buffer;

            _picture->pData[0] = dataStart;
            dataStart += (width * height);

            _picture->pData[1] = dataStart;
            dataStart += (width * height) >> 2;

            _picture->pData[2] = dataStart;
            dataStart += (width * height) >> 2;

            _picture->pData[3] = null;
        }

        protected override int GetDataLength(int index)
        {
            switch (index)
            {
                case 0:
                    return PicWidth * PicHeight;

                case 1:
                case 2:
                    return (PicWidth * PicHeight) >> 2;

                default:
                    return 0;
            }
        }
    }
}
