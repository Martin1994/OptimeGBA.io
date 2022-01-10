namespace OptimeGBAServer.Media.LibVpx.Native
{
    /*!\brief Control functions
     *
     * The set of macros define the control functions of VP8 interface
     */
    enum vp8_com_control_id {
        /*!\brief pass in an external frame into decoder to be used as reference frame
         */
        VP8_SET_REFERENCE = 1,
        VP8_COPY_REFERENCE = 2, /**< get a copy of reference frame from the decoder */
        VP8_SET_POSTPROC = 3,   /**< set the decoder's post processing settings  */

        /* TODO(jkoleszar): The encoder incorrectly reuses some of these values (5+)
         * for its control ids. These should be migrated to something like the
         * VP8_DECODER_CTRL_ID_START range next time we're ready to break the ABI.
         */
        VP9_GET_REFERENCE = 128, /**< get a pointer to a reference frame */
        VP8_COMMON_CTRL_ID_MAX,
        VP8_DECODER_CTRL_ID_START = 256
    }
}
