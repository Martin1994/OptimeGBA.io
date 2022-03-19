namespace OptimeGBAServer.Media.LibX264
{
    /// <summary>
    /// Slice type
    /// </summary>
    public enum X264Type
    {
        AUTO        = 0x0000,  /* Let x264 choose the right type */
        IDR         = 0x0001,
        I           = 0x0002,
        P           = 0x0003,
        BREF        = 0x0004,  /* Non-disposable B-frame */
        B           = 0x0005,
        KEYFRAME    = 0x0006,  /* IDR or I depending on b_open_gop option */
    }

    public static class X264TypeExtension
    {
        public static bool IsTypeI(this X264Type type)
        {
            return type == X264Type.I || type == X264Type.IDR || type == X264Type.KEYFRAME;
        }

        public static bool IsTypeB(this X264Type type)
        {
            return type == X264Type.B || type == X264Type.BREF;
        }
    }
}
