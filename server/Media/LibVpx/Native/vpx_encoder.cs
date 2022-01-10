using System;
using System.Runtime.InteropServices;

namespace OptimeGBAServer.Media.LibVpx.Native
{
    public static unsafe class vpx_encoder
    {
        /*! Spatial Scalability: Maximum number of coding layers */
        public const int VPX_SS_MAX_LAYERS = 5;

        /*! Temporal Scalability: Maximum number of coding layers */
        public const int VPX_TS_MAX_LAYERS = 5;

        /*! Temporal Scalability: Maximum length of the sequence defining frame
         * layer membership
         */
        public const int VPX_TS_MAX_PERIODICITY = 16;

        /*! Temporal+Spatial Scalability: Maximum number of coding layers */
        public const int VPX_MAX_LAYERS = 12;  // 3 temporal + 4 spatial layers are allowed.

        /*!\brief Current ABI version number
         *
         * \internal
         * If this file is altered in any way that changes the ABI, this value
         * must be bumped.  Examples include, but are not limited to, changing
         * types, removing or reassigning enums, adding/removing/rearranging
         * fields to structures
         */
        public const int VPX_ENCODER_ABI_VERSION = (15 + vpx_codec.VPX_CODEC_ABI_VERSION + vpx_ext_ratectrl.VPX_EXT_RATECTRL_ABI_VERSION);

        /*!\brief Get a default configuration
         *
         * Initializes a encoder configuration structure with default values. Supports
         * the notion of "usages" so that an algorithm may offer different default
         * settings depending on the user's intended goal. This function \ref SHOULD
         * be called by all applications to initialize the configuration structure
         * before specializing the configuration with application specific values.
         *
         * \param[in]    iface     Pointer to the algorithm interface to use.
         * \param[out]   cfg       Configuration buffer to populate.
         * \param[in]    usage     Must be set to 0.
         *
         * \retval #VPX_CODEC_OK
         *     The configuration was populated.
         * \retval #VPX_CODEC_INCAPABLE
         *     Interface is not an encoder interface.
         * \retval #VPX_CODEC_INVALID_PARAM
         *     A parameter was NULL, or the usage value was not recognized.
         */
        [DllImport("vpx")]
        public static extern vpx_codec_err_t vpx_codec_enc_config_default(vpx_codec_iface_t* iface, vpx_codec_enc_cfg_t* cfg, uint usage);

        /*!\brief Initialize an encoder instance
         *
         * Initializes a encoder context using the given interface. Applications
         * should call the vpx_codec_enc_init convenience macro instead of this
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
         *                       VPX_ENCODER_ABI_VERSION
         * \retval #VPX_CODEC_OK
         *     The decoder algorithm initialized.
         * \retval #VPX_CODEC_MEM_ERROR
         *     Memory allocation failed.
         */
        [DllImport("vpx")]
        private static extern vpx_codec_err_t vpx_codec_enc_init_ver(vpx_codec_ctx_t* ctx, vpx_codec_iface_t* iface, vpx_codec_enc_cfg_t* cfg, vpx_codec_flags_t flags, int ver);

        /*!\brief Convenience macro for vpx_codec_enc_init_ver()
         *
         * Ensures the ABI version parameter is properly set.
         */
        public static vpx_codec_err_t vpx_codec_enc_init(vpx_codec_ctx_t* ctx, vpx_codec_iface_t* iface, vpx_codec_enc_cfg_t* cfg, vpx_codec_flags_t flags)
            => vpx_codec_enc_init_ver(ctx, iface, cfg, flags, VPX_ENCODER_ABI_VERSION);

        /*!\brief Encode a frame
         *
         * Encodes a video frame at the given "presentation time." The presentation
         * time stamp (PTS) \ref MUST be strictly increasing.
         *
         * The encoder supports the notion of a soft real-time deadline. Given a
         * non-zero value to the deadline parameter, the encoder will make a "best
         * effort" guarantee to  return before the given time slice expires. It is
         * implicit that limiting the available time to encode will degrade the
         * output quality. The encoder can be given an unlimited time to produce the
         * best possible frame by specifying a deadline of '0'. This deadline
         * supersedes the VPx notion of "best quality, good quality, realtime".
         * Applications that wish to map these former settings to the new deadline
         * based system can use the symbols #VPX_DL_REALTIME, #VPX_DL_GOOD_QUALITY,
         * and #VPX_DL_BEST_QUALITY.
         *
         * When the last frame has been passed to the encoder, this function should
         * continue to be called, with the img parameter set to NULL. This will
         * signal the end-of-stream condition to the encoder and allow it to encode
         * any held buffers. Encoding is complete when vpx_codec_encode() is called
         * and vpx_codec_get_cx_data() returns no data.
         *
         * \param[in]    ctx       Pointer to this instance's context
         * \param[in]    img       Image data to encode, NULL to flush.
         * \param[in]    pts       Presentation time stamp, in timebase units.
         * \param[in]    duration  Duration to show frame, in timebase units.
         * \param[in]    flags     Flags to use for encoding this frame.
         * \param[in]    deadline  Time to spend encoding, in microseconds. (0=infinite)
         *
         * \retval #VPX_CODEC_OK
         *     The configuration was populated.
         * \retval #VPX_CODEC_INCAPABLE
         *     Interface is not an encoder interface.
         * \retval #VPX_CODEC_INVALID_PARAM
         *     A parameter was NULL, the image format is unsupported, etc.
         */
        [DllImport("vpx")]
        public static extern vpx_codec_err_t vpx_codec_encode(
            vpx_codec_ctx_t* ctx,
            vpx_image_t* img,
            vpx_codec_pts_t pts,
            uint duration,
            vpx_enc_frame_flags_t flags,
            vpx_enc_deadline_flags_t deadline);

        /*!\brief Encoded data iterator
         *
         * Iterates over a list of data packets to be passed from the encoder to the
         * application. The different kinds of packets available are enumerated in
         * #vpx_codec_cx_pkt_kind.
         *
         * #VPX_CODEC_CX_FRAME_PKT packets should be passed to the application's
         * muxer. Multiple compressed frames may be in the list.
         * #VPX_CODEC_STATS_PKT packets should be appended to a global buffer.
         *
         * The application \ref MUST silently ignore any packet kinds that it does
         * not recognize or support.
         *
         * The data buffers returned from this function are only guaranteed to be
         * valid until the application makes another call to any vpx_codec_* function.
         *
         * \param[in]     ctx      Pointer to this instance's context
         * \param[in,out] iter     Iterator storage, initialized to NULL
         *
         * \return Returns a pointer to an output data packet (compressed frame data,
         *         two-pass statistics, etc.) or NULL to signal end-of-list.
         *
         */
        [DllImport("vpx")]
        public static extern vpx_codec_cx_pkt_t* vpx_codec_get_cx_data(vpx_codec_ctx_t* ctx, vpx_codec_iter_t* iter);
    }

    /*!\brief Encoder configuration structure
     *
     * This structure contains the encoder settings that have common representations
     * across all codecs. This doesn't imply that all codecs support all features,
     * however.
     */
    public unsafe struct vpx_codec_enc_cfg_t
    {
        /*
         * generic settings (g)
         */

        /*!\brief Deprecated: Algorithm specific "usage" value
         *
         * This value must be zero.
         */
        public uint g_usage;

        /*!\brief Maximum number of threads to use
         *
         * For multi-threaded implementations, use no more than this number of
         * threads. The codec may use fewer threads than allowed. The value
         * 0 is equivalent to the value 1.
         */
        public uint g_threads;

        /*!\brief Bitstream profile to use
         *
         * Some codecs support a notion of multiple bitstream profiles. Typically
         * this maps to a set of features that are turned on or off. Often the
         * profile to use is determined by the features of the intended decoder.
         * Consult the documentation for the codec to determine the valid values
         * for this parameter, or set to zero for a sane default.
         */
        public uint g_profile; /**< profile of bitstream to use */

        /*!\brief Width of the frame
         *
         * This value identifies the presentation resolution of the frame,
         * in pixels. Note that the frames passed as input to the encoder must
         * have this resolution. Frames will be presented by the decoder in this
         * resolution, independent of any spatial resampling the encoder may do.
         */
        public uint g_w;

        /*!\brief Height of the frame
         *
         * This value identifies the presentation resolution of the frame,
         * in pixels. Note that the frames passed as input to the encoder must
         * have this resolution. Frames will be presented by the decoder in this
         * resolution, independent of any spatial resampling the encoder may do.
         */
        public uint g_h;

        /*!\brief Bit-depth of the codec
         *
         * This value identifies the bit_depth of the codec,
         * Only certain bit-depths are supported as identified in the
         * vpx_bit_depth_t enum.
         */
        public vpx_bit_depth_t g_bit_depth;

        /*!\brief Bit-depth of the input frames
         *
         * This value identifies the bit_depth of the input frames in bits.
         * Note that the frames passed as input to the encoder must have
         * this bit-depth.
         */
        public uint g_input_bit_depth;

        /*!\brief Stream timebase units
         *
         * Indicates the smallest interval of time, in seconds, used by the stream.
         * For fixed frame rate material, or variable frame rate material where
         * frames are timed at a multiple of a given clock (ex: video capture),
         * the \ref RECOMMENDED method is to set the timebase to the reciprocal
         * of the frame rate (ex: 1001/30000 for 29.970 Hz NTSC). This allows the
         * pts to correspond to the frame number, which can be handy. For
         * re-encoding video from containers with absolute time timestamps, the
         * \ref RECOMMENDED method is to set the timebase to that of the parent
         * container or multimedia framework (ex: 1/1000 for ms, as in FLV).
         */
        public vpx_rational_t g_timebase;

        /*!\brief Enable error resilient modes.
         *
         * The error resilient bitfield indicates to the encoder which features
         * it should enable to take measures for streaming over lossy or noisy
         * links.
         */
        public vpx_codec_er_flags_t g_error_resilient;

        /*!\brief Multi-pass Encoding Mode
         *
         * This value should be set to the current phase for multi-pass encoding.
         * For single pass, set to #VPX_RC_ONE_PASS.
         */
        public vpx_enc_pass g_pass;

        /*!\brief Allow lagged encoding
         *
         * If set, this value allows the encoder to consume a number of input
         * frames before producing output frames. This allows the encoder to
         * base decisions for the current frame on future frames. This does
         * increase the latency of the encoding pipeline, so it is not appropriate
         * in all situations (ex: realtime encoding).
         *
         * Note that this is a maximum value -- the encoder may produce frames
         * sooner than the given limit. Set this value to 0 to disable this
         * feature.
         */
        public uint g_lag_in_frames;

        /*
         * rate control settings (rc)
         */

        /*!\brief Temporal resampling configuration, if supported by the codec.
         *
         * Temporal resampling allows the codec to "drop" frames as a strategy to
         * meet its target data rate. This can cause temporal discontinuities in
         * the encoded video, which may appear as stuttering during playback. This
         * trade-off is often acceptable, but for many applications is not. It can
         * be disabled in these cases.
         *
         * This threshold is described as a percentage of the target data buffer.
         * When the data buffer falls below this percentage of fullness, a
         * dropped frame is indicated. Set the threshold to zero (0) to disable
         * this feature.
         */
        public uint rc_dropframe_thresh;

        /*!\brief Enable/disable spatial resampling, if supported by the codec.
         *
         * Spatial resampling allows the codec to compress a lower resolution
         * version of the frame, which is then upscaled by the encoder to the
         * correct presentation resolution. This increases visual quality at
         * low data rates, at the expense of CPU time on the encoder/decoder.
         */
        public uint rc_resize_allowed;

        /*!\brief Internal coded frame width.
         *
         * If spatial resampling is enabled this specifies the width of the
         * encoded frame.
         */
        public uint rc_scaled_width;

        /*!\brief Internal coded frame height.
         *
         * If spatial resampling is enabled this specifies the height of the
         * encoded frame.
         */
        public uint rc_scaled_height;

        /*!\brief Spatial resampling up watermark.
         *
         * This threshold is described as a percentage of the target data buffer.
         * When the data buffer rises above this percentage of fullness, the
         * encoder will step up to a higher resolution version of the frame.
         */
        public uint rc_resize_up_thresh;

        /*!\brief Spatial resampling down watermark.
         *
         * This threshold is described as a percentage of the target data buffer.
         * When the data buffer falls below this percentage of fullness, the
         * encoder will step down to a lower resolution version of the frame.
         */
        public uint rc_resize_down_thresh;

        /*!\brief Rate control algorithm to use.
         *
         * Indicates whether the end usage of this stream is to be streamed over
         * a bandwidth constrained link, indicating that Constant Bit Rate (CBR)
         * mode should be used, or whether it will be played back on a high
         * bandwidth link, as from a local disk, where higher variations in
         * bitrate are acceptable.
         */
        public vpx_rc_mode rc_end_usage;

        /*!\brief Two-pass stats buffer.
         *
         * A buffer containing all of the stats packets produced in the first
         * pass, concatenated.
         */
        public vpx_fixed_buf_t rc_twopass_stats_in;

        /*!\brief first pass mb stats buffer.
         *
         * A buffer containing all of the first pass mb stats packets produced
         * in the first pass, concatenated.
         */
        public vpx_fixed_buf_t rc_firstpass_mb_stats_in;

        /*!\brief Target data rate
         *
         * Target bitrate to use for this stream, in kilobits per second.
         */
        public uint rc_target_bitrate;

        /*
         * quantizer settings
         */

        /*!\brief Minimum (Best Quality) Quantizer
         *
         * The quantizer is the most direct control over the quality of the
         * encoded image. The range of valid values for the quantizer is codec
         * specific. Consult the documentation for the codec to determine the
         * values to use.
         */
        public uint rc_min_quantizer;

        /*!\brief Maximum (Worst Quality) Quantizer
         *
         * The quantizer is the most direct control over the quality of the
         * encoded image. The range of valid values for the quantizer is codec
         * specific. Consult the documentation for the codec to determine the
         * values to use.
         */
        public uint rc_max_quantizer;

        /*
         * bitrate tolerance
         */

        /*!\brief Rate control adaptation undershoot control
         *
         * VP8: Expressed as a percentage of the target bitrate,
         * controls the maximum allowed adaptation speed of the codec.
         * This factor controls the maximum amount of bits that can
         * be subtracted from the target bitrate in order to compensate
         * for prior overshoot.
         * VP9: Expressed as a percentage of the target bitrate, a threshold
         * undershoot level (current rate vs target) beyond which more aggressive
         * corrective measures are taken.
         *   *
         * Valid values in the range VP8:0-100 VP9: 0-100.
         */
        public uint rc_undershoot_pct;

        /*!\brief Rate control adaptation overshoot control
         *
         * VP8: Expressed as a percentage of the target bitrate,
         * controls the maximum allowed adaptation speed of the codec.
         * This factor controls the maximum amount of bits that can
         * be added to the target bitrate in order to compensate for
         * prior undershoot.
         * VP9: Expressed as a percentage of the target bitrate, a threshold
         * overshoot level (current rate vs target) beyond which more aggressive
         * corrective measures are taken.
         *
         * Valid values in the range VP8:0-100 VP9: 0-100.
         */
        public uint rc_overshoot_pct;

        /*
         * decoder buffer model parameters
         */

        /*!\brief Decoder Buffer Size
         *
         * This value indicates the amount of data that may be buffered by the
         * decoding application. Note that this value is expressed in units of
         * time (milliseconds). For example, a value of 5000 indicates that the
         * client will buffer (at least) 5000ms worth of encoded data. Use the
         * target bitrate (#rc_target_bitrate) to convert to bits/bytes, if
         * necessary.
         */
        public uint rc_buf_sz;

        /*!\brief Decoder Buffer Initial Size
         *
         * This value indicates the amount of data that will be buffered by the
         * decoding application prior to beginning playback. This value is
         * expressed in units of time (milliseconds). Use the target bitrate
         * (#rc_target_bitrate) to convert to bits/bytes, if necessary.
         */
        public uint rc_buf_initial_sz;

        /*!\brief Decoder Buffer Optimal Size
         *
         * This value indicates the amount of data that the encoder should try
         * to maintain in the decoder's buffer. This value is expressed in units
         * of time (milliseconds). Use the target bitrate (#rc_target_bitrate)
         * to convert to bits/bytes, if necessary.
         */
        public uint rc_buf_optimal_sz;

        /*
         * 2 pass rate control parameters
         */

        /*!\brief Two-pass mode CBR/VBR bias
         *
         * Bias, expressed on a scale of 0 to 100, for determining target size
         * for the current frame. The value 0 indicates the optimal CBR mode
         * value should be used. The value 100 indicates the optimal VBR mode
         * value should be used. Values in between indicate which way the
         * encoder should "lean."
         */
        public uint rc_2pass_vbr_bias_pct;

        /*!\brief Two-pass mode per-GOP minimum bitrate
         *
         * This value, expressed as a percentage of the target bitrate, indicates
         * the minimum bitrate to be used for a single GOP (aka "section")
         */
        public uint rc_2pass_vbr_minsection_pct;

        /*!\brief Two-pass mode per-GOP maximum bitrate
         *
         * This value, expressed as a percentage of the target bitrate, indicates
         * the maximum bitrate to be used for a single GOP (aka "section")
         */
        public uint rc_2pass_vbr_maxsection_pct;

        /*!\brief Two-pass corpus vbr mode complexity control
         * Used only in VP9: A value representing the corpus midpoint complexity
         * for corpus vbr mode. This value defaults to 0 which disables corpus vbr
         * mode in favour of normal vbr mode.
         */
        public uint rc_2pass_vbr_corpus_complexity;

        /*
         * keyframing settings (kf)
         */

        /*!\brief Keyframe placement mode
         *
         * This value indicates whether the encoder should place keyframes at a
         * fixed interval, or determine the optimal placement automatically
         * (as governed by the #kf_min_dist and #kf_max_dist parameters)
         */
        public vpx_kf_mode kf_mode;

        /*!\brief Keyframe minimum interval
         *
         * This value, expressed as a number of frames, prevents the encoder from
         * placing a keyframe nearer than kf_min_dist to the previous keyframe. At
         * least kf_min_dist frames non-keyframes will be coded before the next
         * keyframe. Set kf_min_dist equal to kf_max_dist for a fixed interval.
         */
        public uint kf_min_dist;

        /*!\brief Keyframe maximum interval
         *
         * This value, expressed as a number of frames, forces the encoder to code
         * a keyframe if one has not been coded in the last kf_max_dist frames.
         * A value of 0 implies all frames will be keyframes. Set kf_min_dist
         * equal to kf_max_dist for a fixed interval.
         */
        public uint kf_max_dist;

        /*
         * Spatial scalability settings (ss)
         */

        /*!\brief Number of spatial coding layers.
         *
         * This value specifies the number of spatial coding layers to be used.
         */
        public uint ss_number_layers;

        /*!\brief Enable auto alt reference flags for each spatial layer.
         *
         * These values specify if auto alt reference frame is enabled for each
         * spatial layer.
         */
        public fixed int ss_enable_auto_alt_ref[vpx_encoder.VPX_SS_MAX_LAYERS];

        /*!\brief Target bitrate for each spatial layer.
         *
         * These values specify the target coding bitrate to be used for each
         * spatial layer. (in kbps)
         */
        public fixed uint ss_target_bitrate[vpx_encoder.VPX_SS_MAX_LAYERS];

        /*!\brief Number of temporal coding layers.
         *
         * This value specifies the number of temporal layers to be used.
         */
        public uint ts_number_layers;

        /*!\brief Target bitrate for each temporal layer.
         *
         * These values specify the target coding bitrate to be used for each
         * temporal layer. (in kbps)
         */
        public fixed uint ts_target_bitrate[vpx_encoder.VPX_TS_MAX_LAYERS];

        /*!\brief Frame rate decimation factor for each temporal layer.
         *
         * These values specify the frame rate decimation factors to apply
         * to each temporal layer.
         */
        public fixed uint ts_rate_decimator[vpx_encoder.VPX_TS_MAX_LAYERS];

        /*!\brief Length of the sequence defining frame temporal layer membership.
         *
         * This value specifies the length of the sequence that defines the
         * membership of frames to temporal layers. For example, if the
         * ts_periodicity = 8, then the frames are assigned to coding layers with a
         * repeated sequence of length 8.
         */
        public uint ts_periodicity;

        /*!\brief Template defining the membership of frames to temporal layers.
         *
         * This array defines the membership of frames to temporal coding layers.
         * For a 2-layer encoding that assigns even numbered frames to one temporal
         * layer (0) and odd numbered frames to a second temporal layer (1) with
         * ts_periodicity=8, then ts_layer_id = (0,1,0,1,0,1,0,1).
         */
        public fixed uint ts_layer_id[vpx_encoder.VPX_TS_MAX_PERIODICITY];

        /*!\brief Target bitrate for each spatial/temporal layer.
         *
         * These values specify the target coding bitrate to be used for each
         * spatial/temporal layer. (in kbps)
         *
         */
        public fixed uint layer_target_bitrate[vpx_encoder.VPX_MAX_LAYERS];

        /*!\brief Temporal layering mode indicating which temporal layering scheme to
         * use.
         *
         * The value (refer to VP9E_TEMPORAL_LAYERING_MODE) specifies the
         * temporal layering mode to use.
         *
         */
        public int temporal_layering_mode;

        /*!\brief A flag indicating whether to use external rate control parameters.
         * By default is 0. If set to 1, the following parameters will be used in the
         * rate control system.
         */
        public int use_vizier_rc_params;

        /*!\brief Active worst quality factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t active_wq_factor;

        /*!\brief Error per macroblock adjustment factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t err_per_mb_factor;

        /*!\brief Second reference default decay limit.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t sr_default_decay_limit;

        /*!\brief Second reference difference factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t sr_diff_factor;

        /*!\brief Keyframe error per macroblock adjustment factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t kf_err_per_mb_factor;

        /*!\brief Keyframe minimum boost adjustment factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t kf_frame_min_boost_factor;

        /*!\brief Keyframe maximum boost adjustment factor, for the first keyframe
         * in a chunk.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t kf_frame_max_boost_first_factor;

        /*!\brief Keyframe maximum boost adjustment factor, for subsequent keyframes.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t kf_frame_max_boost_subs_factor;

        /*!\brief Keyframe maximum total boost adjustment factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t kf_max_total_boost_factor;

        /*!\brief Golden frame maximum total boost adjustment factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t gf_max_total_boost_factor;

        /*!\brief Golden frame maximum boost adjustment factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t gf_frame_max_boost_factor;

        /*!\brief Zero motion power factor.
         *
         * Rate control parameters, set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t zm_factor;

        /*!\brief Rate-distortion multiplier for inter frames.
         * The multiplier is a crucial parameter in the calculation of rate distortion
         * cost. It is often related to the qp (qindex) value.
         * Rate control parameters, could be set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t rd_mult_inter_qp_fac;

        /*!\brief Rate-distortion multiplier for alt-ref frames.
         * The multiplier is a crucial parameter in the calculation of rate distortion
         * cost. It is often related to the qp (qindex) value.
         * Rate control parameters, could be set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t rd_mult_arf_qp_fac;

        /*!\brief Rate-distortion multiplier for key frames.
         * The multiplier is a crucial parameter in the calculation of rate distortion
         * cost. It is often related to the qp (qindex) value.
         * Rate control parameters, could be set from external experiment results.
         * Only when |use_vizier_rc_params| is set to 1, the pass in value will be
         * used. Otherwise, the default value is used.
         *
         */
        public vpx_rational_t rd_mult_key_qp_fac;
    }

    /*!\brief Bit depth for codec
     * *
     * This enumeration determines the bit depth of the codec.
     */
    public enum vpx_bit_depth_t
    {
        VPX_BITS_8 = 8,   /**<  8 bits */
        VPX_BITS_10 = 10, /**< 10 bits */
        VPX_BITS_12 = 12, /**< 12 bits */
    }

    /*!\brief Rational Number
     *
     * This structure holds a fractional value.
     */
    public struct vpx_rational_t
    {
        public int num;        /**< fraction numerator */
        public int den;        /**< fraction denominator */
    }

    /*!\brief Error Resilient flags
     *
     * These flags define which error resilient features to enable in the
     * encoder. The flags are specified through the
     * vpx_codec_enc_cfg::g_error_resilient variable.
     */
    public enum vpx_codec_er_flags_t
    {
        VPX_ERROR_RESILIENT_NONE = 0x0,
        /*!\brief Improve resiliency against losses of whole frames */
        VPX_ERROR_RESILIENT_DEFAULT = 0x1,
        /*!\brief The frame partitions are independently decodable by the bool decoder,
        * meaning that partitions can be decoded even though earlier partitions have
        * been lost. Note that intra prediction is still done over the partition
        * boundary. */
        VPX_ERROR_RESILIENT_PARTITIONS = 0x2
    }

    /*!\brief Multi-pass Encoding Pass */
    public enum vpx_enc_pass
    {
        VPX_RC_ONE_PASS,   /**< Single pass mode */
        VPX_RC_FIRST_PASS, /**< First pass of multi-pass mode */
        VPX_RC_LAST_PASS   /**< Final pass of multi-pass mode */
    }

    /*!\brief Rate control mode */
    public enum vpx_rc_mode
    {
        VPX_VBR, /**< Variable Bit Rate (VBR) mode */
        VPX_CBR, /**< Constant Bit Rate (CBR) mode */
        VPX_CQ,  /**< Constrained Quality (CQ)  mode */
        VPX_Q,   /**< Constant Quality (Q) mode */
    }

    /*!\brief Generic fixed size buffer structure
     *
     * This structure is able to hold a reference to any fixed size buffer.
     */
    public unsafe struct vpx_fixed_buf_t
    {
        public byte* buf;       /**< Pointer to the data */
        public ulong sz;       /**< Length of the buffer, in chars */
    }

    /*!\brief Keyframe placement mode.
     *
     * This enumeration determines whether keyframes are placed automatically by
     * the encoder or whether this behavior is disabled. Older releases of this
     * SDK were implemented such that VPX_KF_FIXED meant keyframes were disabled.
     * This name is confusing for this behavior, so the new symbols to be used
     * are VPX_KF_AUTO and VPX_KF_DISABLED.
     */
    public enum vpx_kf_mode
    {
        VPX_KF_FIXED,       /**< deprecated, implies VPX_KF_DISABLED */
        VPX_KF_AUTO,        /**< Encoder determines optimal placement automatically */
        VPX_KF_DISABLED = 0 /**< Encoder does not place keyframes. */
    }

    /*!\brief Encoder output packet
     *
     * This structure contains the different kinds of output data the encoder
     * may produce while compressing a frame.
     */
    public unsafe struct vpx_codec_cx_pkt_t
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct data_t
        {
            [FieldOffset(0)]
            public frame_t frame;                      /**< data for compressed frame packet */

            [FieldOffset(0)]
            public vpx_fixed_buf_t twopass_stats;      /**< data for two-pass packet */

            [FieldOffset(0)]
            public vpx_fixed_buf_t firstpass_mb_stats; /**< first pass mb packet */

            [FieldOffset(0)]
            public vpx_psnr_pkt_t psnr;                /**< data for PSNR packet */

            [FieldOffset(0)]
            public vpx_fixed_buf_t raw;       /**< data for arbitrary packets */

            #pragma warning disable 0169
            /* This packet size is fixed to allow codecs to extend this
            * interface without having to manage storage for raw packets,
            * i.e., if it's smaller than 128 bytes, you can store in the
            * packet list directly.
            */
            [FieldOffset(0)]
            private fixed byte pad[128 - sizeof(vpx_codec_cx_pkt_kind)]; /**< fixed sz */
            #pragma warning restore 0169
        }

        public struct frame_t
        {
            public byte* buf; /**< compressed data buffer */
            public ulong sz; /**< length of compressed data */
            /*!\brief time stamp to show frame (in timebase units) */
            public vpx_codec_pts_t pts;
            /*!\brief duration to show frame (in timebase units) */
            public uint duration;
            public vpx_codec_frame_flags_t flags; /**< flags for this frame */
            /*!\brief the partition id defines the decoding order of the partitions.
            * Only applicable when "output partition" mode is enabled. First
            * partition has id 0.*/
            public int partition_id;
            /*!\brief Width and height of frames in this packet. VP8 will only use the
            * first one.*/
            public fixed uint width[vpx_encoder.VPX_SS_MAX_LAYERS];  /**< frame width */
            public fixed uint height[vpx_encoder.VPX_SS_MAX_LAYERS]; /**< frame height */
            /*!\brief Flag to indicate if spatial layer frame in this packet is
            * encoded or dropped. VP8 will always be set to 1.*/
            public fixed byte spatial_layer_encoded[vpx_encoder.VPX_SS_MAX_LAYERS];
        }

        public struct vpx_psnr_pkt_t
        {
            public fixed uint samples[4]; /**< Number of samples, total/y/u/v */
            public fixed ulong sse[4];         /**< sum squared error, total/y/u/v */
            public fixed double psnr[4];          /**< PSNR, total/y/u/v */
        }

        public vpx_codec_cx_pkt_kind kind; /**< packet variant */
        public data_t data; /**< packet data */
    }

    /*!\brief Encoder output packet variants
     *
     * This enumeration lists the different kinds of data packets that can be
     * returned by calls to vpx_codec_get_cx_data(). Algorithms \ref MAY
     * extend this list to provide additional functionality.
     */
    public enum vpx_codec_cx_pkt_kind
    {
        VPX_CODEC_CX_FRAME_PKT,    /**< Compressed video frame */
        VPX_CODEC_STATS_PKT,       /**< Two-pass statistics for this frame */
        VPX_CODEC_FPMB_STATS_PKT,  /**< first pass mb statistics for this frame */
        VPX_CODEC_PSNR_PKT,        /**< PSNR statistics for this frame */
        VPX_CODEC_CUSTOM_PKT = 256 /**< Algorithm extensions  */
    }

    /*!\brief Time Stamp Type
     *
     * An integer, which when multiplied by the stream's time base, provides
     * the absolute time of a sample.
     */
    public struct vpx_codec_pts_t
    {
        public long value;

        public static implicit operator vpx_codec_pts_t(long value) => new vpx_codec_pts_t() { value = value };
        public static implicit operator long(vpx_codec_pts_t t) => t.value;
    }

    /*!\brief Compressed Frame Flags
     *
     * This type represents a bitfield containing information about a compressed
     * frame that may be useful to an application. The most significant 16 bits
     * can be used by an algorithm to provide additional detail, for example to
     * support frame types that are codec specific (MPEG-1 D-frames for example)
     */
    public enum vpx_codec_frame_flags_t
    {
        VPX_FRAME_NONE = 0x0,
        VPX_FRAME_IS_KEY = 0x1, /**< frame is the start of a GOP */
        /*!\brief frame can be dropped without affecting the stream (no future frame
         * depends on this one) */
        VPX_FRAME_IS_DROPPABLE = 0x2,
        /*!\brief frame should be decoded but will not be shown */
        VPX_FRAME_IS_INVISIBLE = 0x4,
        /*!\brief this is a fragment of the encoded frame */
        VPX_FRAME_IS_FRAGMENT = 0x8
    }

    public enum vpx_enc_frame_flags_t
    {
        VPX_EFLAG_NONE,
        VPX_EFLAG_FORCE_KF
    }

    public enum vpx_enc_deadline_flags_t
    {
        /*!\brief deadline parameter analogous to VPx REALTIME mode. */
        VPX_DL_REALTIME = 1,
        /*!\brief deadline parameter analogous to  VPx GOOD QUALITY mode. */
        VPX_DL_GOOD_QUALITY = 1000000,
        /*!\brief deadline parameter analogous to VPx BEST QUALITY mode. */
        VPX_DL_BEST_QUALITY = 0
    }
}
