using System;
using System.Runtime.InteropServices;

namespace OptimeGBAServer.Media.LibVpx.Native
{

    public static unsafe class vpx_image
    {
        public const int VPX_IMAGE_ABI_VERSION = 5;

        /*!\brief Open a descriptor, allocating storage for the underlying image
         *
         * Returns a descriptor for storing an image of the given format. The
         * storage for the descriptor is allocated on the heap.
         *
         * \param[in]    img       Pointer to storage for descriptor. If this parameter
         *                         is NULL, the storage for the descriptor will be
         *                         allocated on the heap.
         * \param[in]    fmt       Format for the image
         * \param[in]    d_w       Width of the image
         * \param[in]    d_h       Height of the image
         * \param[in]    align     Alignment, in bytes, of the image buffer and
         *                         each row in the image(stride).
         *
         * \return Returns a pointer to the initialized image descriptor. If the img
         *         parameter is non-null, the value of the img parameter will be
         *         returned.
         */
        [DllImport("vpx")]
        public static extern vpx_image_t* vpx_img_alloc(vpx_image_t *img, vpx_img_fmt_t fmt, uint d_w, uint d_h, uint align);

        /*!\brief Close an image descriptor
         *
         * Frees all allocated storage associated with an image descriptor.
         *
         * \param[in]    img       Image descriptor
         */
        [DllImport("vpx")]
        public static extern void vpx_img_free(vpx_image_t *img);
    }

    public unsafe struct vpx_image_t
    {
        public vpx_img_fmt_t fmt;       /**< Image Format */
        public vpx_color_space_t cs;    /**< Color Space */
        public vpx_color_range_t range; /**< Color Range */

        /* Image storage dimensions */
        public uint w;         /**< Stored image width */
        public uint h;         /**< Stored image height */
        public uint bit_depth; /**< Stored image bit-depth */

        /* Image display dimensions */
        public uint d_w; /**< Displayed image width */
        public uint d_h; /**< Displayed image height */

        /* Image intended rendering dimensions */
        public uint r_w; /**< Intended rendering image width */
        public uint r_h; /**< Intended rendering image height */

        /* Chroma subsampling info */
        public uint x_chroma_shift; /**< subsampling order, X */
        public uint y_chroma_shift; /**< subsampling order, Y */

        /* Image data pointers. */
        /**< pointer to the top left pixel for each plane */
        public byte* plane_y;
        public byte* plane_u;
        public byte* plane_v;
        public byte* plane_alpha;
        /**< stride between rows for each plane */
        public int stride_y;
        public int stride_u;
        public int stride_v;
        public int stride_alpha;

        public int bps; /**< bits per sample (for packed formats) */

        /*!\brief The following member may be set by the application to associate
        * data with this image.
        */
        public IntPtr user_priv;

        #pragma warning disable 0169
        /* The following members should be treated as private. */
        private IntPtr img_data; /**< private */
        private int img_data_owner;      /**< private */
        private int self_allocd;         /**< private */

        private IntPtr fb_priv; /**< Frame buffer data associated with the image. */
        #pragma warning restore 0169
    }

    public enum vpx_img_fmt_t
    {
        VPX_IMG_FMT_NONE,
        VPX_IMG_FMT_YV12 =
            VPX_IMG_FMT_PLANAR | VPX_IMG_FMT_UV_FLIP | 1, /**< planar YVU */
        VPX_IMG_FMT_I420 = VPX_IMG_FMT_PLANAR | 2,
        VPX_IMG_FMT_I422 = VPX_IMG_FMT_PLANAR | 5,
        VPX_IMG_FMT_I444 = VPX_IMG_FMT_PLANAR | 6,
        VPX_IMG_FMT_I440 = VPX_IMG_FMT_PLANAR | 7,
        VPX_IMG_FMT_NV12 = VPX_IMG_FMT_PLANAR | 9,
        VPX_IMG_FMT_I42016 = VPX_IMG_FMT_I420 | VPX_IMG_FMT_HIGHBITDEPTH,
        VPX_IMG_FMT_I42216 = VPX_IMG_FMT_I422 | VPX_IMG_FMT_HIGHBITDEPTH,
        VPX_IMG_FMT_I44416 = VPX_IMG_FMT_I444 | VPX_IMG_FMT_HIGHBITDEPTH,
        VPX_IMG_FMT_I44016 = VPX_IMG_FMT_I440 | VPX_IMG_FMT_HIGHBITDEPTH,
        VPX_IMG_FMT_PLANAR = 0x100,       /**< Image is a planar format. */
        VPX_IMG_FMT_UV_FLIP = 0x200,      /**< V plane precedes U in memory. */
        VPX_IMG_FMT_HAS_ALPHA = 0x400,    /**< Image has an alpha channel. */
        VPX_IMG_FMT_HIGHBITDEPTH = 0x800 /**< Image uses 16bit framebuffer. */
    }

    /*!\brief List of supported color spaces */
    public enum vpx_color_space_t
    {
        VPX_CS_UNKNOWN = 0,   /**< Unknown */
        VPX_CS_BT_601 = 1,    /**< BT.601 */
        VPX_CS_BT_709 = 2,    /**< BT.709 */
        VPX_CS_SMPTE_170 = 3, /**< SMPTE.170 */
        VPX_CS_SMPTE_240 = 4, /**< SMPTE.240 */
        VPX_CS_BT_2020 = 5,   /**< BT.2020 */
        VPX_CS_RESERVED = 6,  /**< Reserved */
        VPX_CS_SRGB = 7       /**< sRGB */
    }

    /*!\brief List of supported color range */
    public enum vpx_color_range_t
    {
        VPX_CR_STUDIO_RANGE = 0, /**< Y [16..235], UV [16..240] */
        VPX_CR_FULL_RANGE = 1    /**< YUV/RGB [0..255] */
    }
}
