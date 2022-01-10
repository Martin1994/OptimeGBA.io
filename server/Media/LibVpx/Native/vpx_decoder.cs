using System.Runtime.InteropServices;

namespace OptimeGBAServer.Media.LibVpx.Native
{
    public static unsafe class vpx_decoder
    {
        public const int VPX_DECODER_ABI_VERSION = 3 + vpx_codec.VPX_CODEC_ABI_VERSION;

        /*!\brief Initialize a decoder instance
         *
         * Initializes a decoder context using the given interface. Applications
         * should call the vpx_codec_dec_init convenience macro instead of this
         * function directly, to ensure that the ABI version number parameter
         * is properly initialized.
         *
         * If the library was configured with --disable-multithread, this call
         * is not thread safe and should be guarded with a lock if being used
         * in a multithreaded context.
         *
         * \param[in]    ctx     Pointer to this instance's context.
         * \param[in]    iface   Pointer to the algorithm interface to use.
         * \param[in]    cfg     Configuration to use, if known. May be NULL.
         * \param[in]    flags   Bitfield of VPX_CODEC_USE_* flags
         * \param[in]    ver     ABI version number. Must be set to
         *                       VPX_DECODER_ABI_VERSION
         * \retval #VPX_CODEC_OK
         *     The decoder algorithm initialized.
         * \retval #VPX_CODEC_MEM_ERROR
         *     Memory allocation failed.
         */
        [DllImport("vpx")]
        private static extern vpx_codec_err_t vpx_codec_dec_init_ver(vpx_codec_ctx_t *ctx, vpx_codec_iface_t *iface, vpx_codec_dec_cfg_t *cfg, vpx_codec_flags_t flags, int ver);

        /*!\brief Convenience macro for vpx_codec_dec_init_ver()
         *
         * Ensures the ABI version parameter is properly set.
         */
        public static vpx_codec_err_t vpx_codec_dec_init(vpx_codec_ctx_t *ctx, vpx_codec_iface_t *iface, vpx_codec_dec_cfg_t *cfg, vpx_codec_flags_t flags)
            => vpx_codec_dec_init_ver(ctx, iface, cfg, flags, VPX_DECODER_ABI_VERSION);

        /*!\brief Decode data
         *
         * Processes a buffer of coded data. If the processing results in a new
         * decoded frame becoming available, put_slice and put_frame callbacks may be
         * invoked, as appropriate. Encoded data \ref MUST be passed in DTS (decode
         * time stamp) order. Frames produced will always be in PTS (presentation
         * time stamp) order.
         * If the decoder is configured with VPX_CODEC_USE_INPUT_FRAGMENTS enabled,
         * data and data_sz can contain a fragment of the encoded frame. Fragment
         * \#n must contain at least partition \#n, but can also contain subsequent
         * partitions (\#n+1 - \#n+i), and if so, fragments \#n+1, .., \#n+i must
         * be empty. When no more data is available, this function should be called
         * with NULL as data and 0 as data_sz. The memory passed to this function
         * must be available until the frame has been decoded.
         *
         * \param[in] ctx          Pointer to this instance's context
         * \param[in] data         Pointer to this block of new coded data. If
         *                         NULL, the put_frame callback is invoked for
         *                         the previously decoded frame.
         * \param[in] data_sz      Size of the coded data, in bytes.
         * \param[in] user_priv    Application specific data to associate with
         *                         this frame.
         * \param[in] deadline     Soft deadline the decoder should attempt to meet,
         *                         in us. Set to zero for unlimited.
         *
         * \return Returns #VPX_CODEC_OK if the coded data was processed completely
         *         and future pictures can be decoded without error. Otherwise,
         *         see the descriptions of the other error codes in ::vpx_codec_err_t
         *         for recoverability capabilities.
         */
        [DllImport("vpx")]
        public static extern vpx_codec_err_t vpx_codec_decode(vpx_codec_ctx_t *ctx, byte *data, uint data_sz, void *user_priv, int deadline);

        /*!\brief Decoded frames iterator
         *
         * Iterates over a list of the frames available for display. The iterator
         * storage should be initialized to NULL to start the iteration. Iteration is
         * complete when this function returns NULL.
         *
         * The list of available frames becomes valid upon completion of the
         * vpx_codec_decode call, and remains valid until the next call to
         * vpx_codec_decode.
         *
         * \param[in]     ctx      Pointer to this instance's context
         * \param[in,out] iter     Iterator storage, initialized to NULL
         *
         * \return Returns a pointer to an image, if one is ready for display. Frames
         *         produced will always be in PTS (presentation time stamp) order.
         */
        [DllImport("vpx")]
        public static extern vpx_image_t *vpx_codec_get_frame(vpx_codec_ctx_t *ctx, vpx_codec_iter_t *iter);
    }

    /*!\brief Initialization Configurations
     *
     * This structure is used to pass init time configuration options to the
     * decoder.
     */
    public struct vpx_codec_dec_cfg_t {
        public uint threads; /**< Maximum number of threads to use, default 1 */
        public uint w;       /**< Width */
        public uint h;       /**< Height */
    }
}
