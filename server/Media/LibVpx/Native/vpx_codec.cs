using System;
using System.Runtime.InteropServices;

namespace OptimeGBAServer.Media.LibVpx.Native
{
    public static unsafe class vpx_codec
    {
        public const int VPX_CODEC_ABI_VERSION = (4 + vpx_image.VPX_IMAGE_ABI_VERSION);

        /*!\brief Return the name for a given interface
         *
         * Returns a human readable string for name of the given codec interface.
         *
         * \param[in]    iface     Interface pointer
         *
         */
        [DllImport("vpx")]
        public static extern byte* vpx_codec_iface_name(vpx_codec_iface_t *iface);

        /*!\brief Control algorithm
         *
         * This function is used to exchange algorithm specific data with the codec
         * instance. This can be used to implement features specific to a particular
         * algorithm.
         *
         * This wrapper function dispatches the request to the helper function
         * associated with the given ctrl_id. It tries to call this function
         * transparently, but will return #VPX_CODEC_ERROR if the request could not
         * be dispatched.
         *
         * Note that this function should not be used directly. Call the
         * #vpx_codec_control wrapper macro instead.
         *
         * \param[in]     ctx              Pointer to this instance's context
         * \param[in]     ctrl_id          Algorithm specific control identifier
         *
         * \retval #VPX_CODEC_OK
         *     The control request was processed.
         * \retval #VPX_CODEC_ERROR
         *     The control request was not processed.
         * \retval #VPX_CODEC_INVALID_PARAM
         *     The data was not valid.
         */
        [DllImport("vpx")]
        public static extern vpx_codec_err_t vpx_codec_control_(vpx_codec_ctx_t *ctx, vp8e_enc_control_id ctrl_id, int arg0);

        /*!\brief Destroy a codec instance
         *
         * Destroys a codec context, freeing any associated memory buffers.
         *
         * \param[in] ctx   Pointer to this instance's context
         *
         * \retval #VPX_CODEC_OK
         *     The codec algorithm initialized.
         * \retval #VPX_CODEC_MEM_ERROR
         *     Memory allocation failed.
         */
        [DllImport("vpx")]
        public static extern vpx_codec_err_t vpx_codec_destroy(vpx_codec_ctx_t *ctx);
    }

    /*!\brief Codec context structure
     *
     * All codecs \ref MUST support this context structure fully. In general,
     * this data should be considered private to the codec algorithm, and
     * not be manipulated or examined by the calling application. Applications
     * may reference the 'name' member to get a printable description of the
     * algorithm.
     */
    public unsafe struct vpx_codec_ctx_t
    {
        public byte* name;             /**< Printable interface name */
        public vpx_codec_iface_t* iface;     /**< Interface pointers */
        public vpx_codec_err_t err;          /**< Last returned error */
        public byte* err_detail;       /**< Detailed info, if available */
        public vpx_codec_flags_t init_flags; /**< Flags passed at init time */
        public IntPtr config;               /**< Configuration pointer aliasing union */
        public IntPtr priv; /**< Algorithm private storage */
    }

    /*!\brief Algorithm return codes */
    public enum vpx_codec_err_t
    {
        /*!\brief Operation completed without error */
        VPX_CODEC_OK,
        /*!\brief Unspecified error */
        VPX_CODEC_ERROR,
        /*!\brief Memory operation failed */
        VPX_CODEC_MEM_ERROR,
        /*!\brief ABI version mismatch */
        VPX_CODEC_ABI_MISMATCH,
        /*!\brief Algorithm does not have required capability */
        VPX_CODEC_INCAPABLE,
        /*!\brief The given bitstream is not supported.
        *
        * The bitstream was unable to be parsed at the highest level. The decoder
        * is unable to proceed. This error \ref SHOULD be treated as fatal to the
        * stream. */
        VPX_CODEC_UNSUP_BITSTREAM,
        /*!\brief Encoded bitstream uses an unsupported feature
        *
        * The decoder does not implement a feature required by the encoder. This
        * return code should only be used for features that prevent future
        * pictures from being properly decoded. This error \ref MAY be treated as
        * fatal to the stream or \ref MAY be treated as fatal to the current GOP.
        */
        VPX_CODEC_UNSUP_FEATURE,
        /*!\brief The coded data for this stream is corrupt or incomplete
        *
        * There was a problem decoding the current frame.  This return code
        * should only be used for failures that prevent future pictures from
        * being properly decoded. This error \ref MAY be treated as fatal to the
        * stream or \ref MAY be treated as fatal to the current GOP. If decoding
        * is continued for the current GOP, artifacts may be present.
        */
        VPX_CODEC_CORRUPT_FRAME,
        /*!\brief An application-supplied parameter is not valid.
        *
        */
        VPX_CODEC_INVALID_PARAM,
        /*!\brief An iterator reached the end of list.
        *
        */
        VPX_CODEC_LIST_END
    }

    /*! \brief Initialization-time Feature Enabling
     *
     *  Certain codec features must be known at initialization time, to allow for
     *  proper memory allocation.
     *
     *  The available flags are specified by VPX_CODEC_USE_* defines.
     */
    [Flags]
    public enum vpx_codec_flags_t
    {
        /* Encoder flags */
        /*! \brief Initialization-time Feature Enabling
         *
         *  Certain codec features must be known at initialization time, to allow for
         *  proper memory allocation.
         *
         *  The available flags are specified by VPX_CODEC_USE_* defines.
         */
        VPX_CODEC_USE_POSTPROC = 0x10000, /**< Postprocess decoded frame */
        /*!\brief Conceal errors in decoded frames */
        VPX_CODEC_USE_ERROR_CONCEALMENT = 0x20000,
        /*!\brief The input frame should be passed to the decoder one fragment at a
        * time */
        VPX_CODEC_USE_INPUT_FRAGMENTS = 0x40000,
        /*!\brief Enable frame-based multi-threading */
        VPX_CODEC_USE_FRAME_THREADING = 0x80000,

        /* Decoder flags */
        /*! \brief Initialization-time Feature Enabling
         *
         *  Certain codec features must be known at initialization time, to allow
         *  for proper memory allocation.
         *
         *  The available flags are specified by VPX_CODEC_USE_* defines.
         */
        VPX_CODEC_USE_PSNR = 0x10000, /**< Calculate PSNR on each frame */
        /*!\brief Make the encoder output one  partition at a time. */
        VPX_CODEC_USE_OUTPUT_PARTITION = 0x20000,
        VPX_CODEC_USE_HIGHBITDEPTH = 0x40000, /**< Use high bitdepth */
    }

    /*!\brief Codec interface structure.
    *
    * Contains function pointers and other data private to the codec
    * implementation. This structure is opaque to the application.
    */
    public struct vpx_codec_iface_t
    {
        // Opaque implementation. Use pointers only.
    }

    /*!\brief Iterator
     *
     * Opaque storage used for iterating over lists.
     */
    public unsafe struct vpx_codec_iter_t
    {
        // Opaque implementation. Use pointers only.
        private void* data;
    }
}
