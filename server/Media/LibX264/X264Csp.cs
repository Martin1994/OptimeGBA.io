namespace OptimeGBAServer.Media.LibX264
{
    /// <summary>
    /// Colorspace type
    /// </summary>
    public enum X264Csp
    {
        MASK         = 0x00ff,  /* */
        NONE         = 0x0000,  /* Invalid mode     */
        I400         = 0x0001,  /* monochrome 4:0:0 */
        I420         = 0x0002,  /* yuv 4:2:0 planar */
        YV12         = 0x0003,  /* yvu 4:2:0 planar */
        NV12         = 0x0004,  /* yuv 4:2:0, with one y plane and one packed u+v */
        NV21         = 0x0005,  /* yuv 4:2:0, with one y plane and one packed v+u */
        I422         = 0x0006,  /* yuv 4:2:2 planar */
        YV16         = 0x0007,  /* yvu 4:2:2 planar */
        NV16         = 0x0008,  /* yuv 4:2:2, with one y plane and one packed u+v */
        YUYV         = 0x0009,  /* yuyv 4:2:2 packed */
        UYVY         = 0x000a,  /* uyvy 4:2:2 packed */
        V210         = 0x000b,  /* 10-bit yuv 4:2:2 packed in 32 */
        I444         = 0x000c,  /* yuv 4:4:4 planar */
        YV24         = 0x000d,  /* yvu 4:4:4 planar */
        BGR          = 0x000e,  /* packed bgr 24bits */
        BGRA         = 0x000f,  /* packed bgr 32bits */
        RGB          = 0x0010,  /* packed rgb 24bits */
        MAX          = 0x0011,  /* end of list */
        VFLIP        = 0x1000,  /* the csp is vertically flipped */
        HIGH_DEPTH   = 0x2000,  /* the csp has a depth of 16 bits per pixel component */
    }
}
