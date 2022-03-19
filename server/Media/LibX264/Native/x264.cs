using System.Runtime.InteropServices;

namespace OptimeGBAServer.Media.LibX264.Native
{
    public partial struct x264_t
    {
    }

    public enum nal_unit_type_e
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
        NAL_AUD = 9,
        NAL_FILLER = 12,
    }

    public enum nal_priority_e
    {
        NAL_PRIORITY_DISPOSABLE = 0,
        NAL_PRIORITY_LOW = 1,
        NAL_PRIORITY_HIGH = 2,
        NAL_PRIORITY_HIGHEST = 3,
    }

    public unsafe partial struct x264_nal_t
    {
        public nal_priority_e i_ref_idc;

        public nal_unit_type_e i_type;

        public int b_long_startcode;

        public int i_first_mb;

        public int i_last_mb;

        public int i_payload;

        [NativeTypeName("uint8_t *")]
        public byte* p_payload;

        public int i_padding;
    }

    public unsafe partial struct x264_zone_t
    {
        public int i_start;

        public int i_end;

        public int b_force_qp;

        public int i_qp;

        public float f_bitrate_factor;

        [NativeTypeName("struct x264_param_t *")]
        public x264_param_t* param;
    }

    public unsafe partial struct x264_param_t
    {
        [NativeTypeName("uint32_t")]
        public uint cpu;

        public int i_threads;

        public int i_lookahead_threads;

        public int b_sliced_threads;

        public int b_deterministic;

        public int b_cpu_independent;

        public int i_sync_lookahead;

        public int i_width;

        public int i_height;

        public X264Csp i_csp;

        public int i_bitdepth;

        public int i_level_idc;

        public int i_frame_total;

        public int i_nal_hrd;

        [NativeTypeName("struct (anonymous struct at x264.h:331:5)")]
        public _vui_e__Struct vui;

        public int i_frame_reference;

        public int i_dpb_size;

        public int i_keyint_max;

        public int i_keyint_min;

        public int i_scenecut_threshold;

        public int b_intra_refresh;

        public int i_bframe;

        public int i_bframe_adaptive;

        public int i_bframe_bias;

        public int i_bframe_pyramid;

        public int b_open_gop;

        public int b_bluray_compat;

        public int i_avcintra_class;

        public int i_avcintra_flavor;

        public int b_deblocking_filter;

        public int i_deblocking_filter_alphac0;

        public int i_deblocking_filter_beta;

        public int b_cabac;

        public int i_cabac_init_idc;

        public int b_interlaced;

        public int b_constrained_intra;

        public int i_cqm_preset;

        [NativeTypeName("char *")]
        public sbyte* psz_cqm_file;

        [NativeTypeName("uint8_t [16]")]
        public fixed byte cqm_4iy[16];

        [NativeTypeName("uint8_t [16]")]
        public fixed byte cqm_4py[16];

        [NativeTypeName("uint8_t [16]")]
        public fixed byte cqm_4ic[16];

        [NativeTypeName("uint8_t [16]")]
        public fixed byte cqm_4pc[16];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte cqm_8iy[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte cqm_8py[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte cqm_8ic[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte cqm_8pc[64];

        [NativeTypeName("void (*)(void *, int, const char *, va_list)")]
        public delegate* unmanaged[Cdecl]<void*, int, sbyte*, sbyte*, void> pf_log;

        public void* p_log_private;

        public int i_log_level;

        public int b_full_recon;

        [NativeTypeName("char *")]
        public sbyte* psz_dump_yuv;

        [NativeTypeName("struct (anonymous struct at x264.h:395:5)")]
        public _analyse_e__Struct analyse;

        [NativeTypeName("struct (anonymous struct at x264.h:432:5)")]
        public _rc_e__Struct rc;

        [NativeTypeName("struct (anonymous struct at x264.h:477:5)")]
        public _crop_rect_e__Struct crop_rect;

        public int i_frame_packing;

        [NativeTypeName("struct (anonymous struct at x264.h:490:5)")]
        public _mastering_display_e__Struct mastering_display;

        [NativeTypeName("struct (anonymous struct at x264.h:506:5)")]
        public _content_light_level_e__Struct content_light_level;

        public int i_alternative_transfer;

        public int b_aud;

        public int b_repeat_headers;

        public int b_annexb;

        public int i_sps_id;

        public int b_vfr_input;

        public int b_pulldown;

        [NativeTypeName("uint32_t")]
        public uint i_fps_num;

        [NativeTypeName("uint32_t")]
        public uint i_fps_den;

        [NativeTypeName("uint32_t")]
        public uint i_timebase_num;

        [NativeTypeName("uint32_t")]
        public uint i_timebase_den;

        public int b_tff;

        public int b_pic_struct;

        public int b_fake_interlaced;

        public int b_stitchable;

        public int b_opencl;

        public int i_opencl_device;

        public void* opencl_device_id;

        [NativeTypeName("char *")]
        public sbyte* psz_clbin_file;

        public int i_slice_max_size;

        public int i_slice_max_mbs;

        public int i_slice_min_mbs;

        public int i_slice_count;

        public int i_slice_count_max;

        [NativeTypeName("void (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> param_free;

        [NativeTypeName("void (*)(x264_t *, x264_nal_t *, void *)")]
        public delegate* unmanaged[Cdecl]<x264_t*, x264_nal_t*, void*, void> nalu_process;

        public void* opaque;

        public partial struct _vui_e__Struct
        {
            public int i_sar_height;

            public int i_sar_width;

            public int i_overscan;

            public int i_vidformat;

            public int b_fullrange;

            public int i_colorprim;

            public int i_transfer;

            public int i_colmatrix;

            public int i_chroma_loc;
        }

        public unsafe partial struct _analyse_e__Struct
        {
            [NativeTypeName("unsigned int")]
            public uint intra;

            [NativeTypeName("unsigned int")]
            public uint inter;

            public int b_transform_8x8;

            public int i_weighted_pred;

            public int b_weighted_bipred;

            public int i_direct_mv_pred;

            public int i_chroma_qp_offset;

            public int i_me_method;

            public int i_me_range;

            public int i_mv_range;

            public int i_mv_range_thread;

            public int i_subpel_refine;

            public int b_chroma_me;

            public int b_mixed_references;

            public int i_trellis;

            public int b_fast_pskip;

            public int b_dct_decimate;

            public int i_noise_reduction;

            public float f_psy_rd;

            public float f_psy_trellis;

            public int b_psy;

            public int b_mb_info;

            public int b_mb_info_update;

            [NativeTypeName("int [2]")]
            public fixed int i_luma_deadzone[2];

            public int b_psnr;

            public int b_ssim;
        }

        public unsafe partial struct _rc_e__Struct
        {
            public int i_rc_method;

            public int i_qp_constant;

            public int i_qp_min;

            public int i_qp_max;

            public int i_qp_step;

            public int i_bitrate;

            public float f_rf_constant;

            public float f_rf_constant_max;

            public float f_rate_tolerance;

            public int i_vbv_max_bitrate;

            public int i_vbv_buffer_size;

            public float f_vbv_buffer_init;

            public float f_ip_factor;

            public float f_pb_factor;

            public int b_filler;

            public int i_aq_mode;

            public float f_aq_strength;

            public int b_mb_tree;

            public int i_lookahead;

            public int b_stat_write;

            [NativeTypeName("char *")]
            public sbyte* psz_stat_out;

            public int b_stat_read;

            [NativeTypeName("char *")]
            public sbyte* psz_stat_in;

            public float f_qcompress;

            public float f_qblur;

            public float f_complexity_blur;

            public x264_zone_t* zones;

            public int i_zones;

            [NativeTypeName("char *")]
            public sbyte* psz_zones;
        }

        public partial struct _crop_rect_e__Struct
        {
            public int i_left;

            public int i_top;

            public int i_right;

            public int i_bottom;
        }

        public partial struct _mastering_display_e__Struct
        {
            public int b_mastering_display;

            public int i_green_x;

            public int i_green_y;

            public int i_blue_x;

            public int i_blue_y;

            public int i_red_x;

            public int i_red_y;

            public int i_white_x;

            public int i_white_y;

            [NativeTypeName("int64_t")]
            public long i_display_max;

            [NativeTypeName("int64_t")]
            public long i_display_min;
        }

        public partial struct _content_light_level_e__Struct
        {
            public int b_cll;

            public int i_max_cll;

            public int i_max_fall;
        }
    }

    public partial struct x264_level_t
    {
        [NativeTypeName("uint8_t")]
        public byte level_idc;

        [NativeTypeName("int32_t")]
        public int mbps;

        [NativeTypeName("int32_t")]
        public int frame_size;

        [NativeTypeName("int32_t")]
        public int dpb;

        [NativeTypeName("int32_t")]
        public int bitrate;

        [NativeTypeName("int32_t")]
        public int cpb;

        [NativeTypeName("uint16_t")]
        public ushort mv_range;

        [NativeTypeName("uint8_t")]
        public byte mvs_per_2mb;

        [NativeTypeName("uint8_t")]
        public byte slice_rate;

        [NativeTypeName("uint8_t")]
        public byte mincr;

        [NativeTypeName("uint8_t")]
        public byte bipred8x8;

        [NativeTypeName("uint8_t")]
        public byte direct8x8;

        [NativeTypeName("uint8_t")]
        public byte frame_only;
    }

    public enum pic_struct_e
    {
        PIC_STRUCT_AUTO = 0,
        PIC_STRUCT_PROGRESSIVE = 1,
        PIC_STRUCT_TOP_BOTTOM = 4,
        PIC_STRUCT_BOTTOM_TOP = 5,
        PIC_STRUCT_TOP_BOTTOM_TOP = 6,
        PIC_STRUCT_BOTTOM_TOP_BOTTOM = 7,
        PIC_STRUCT_DOUBLE = 8,
        PIC_STRUCT_TRIPLE = 9,
    }

    public partial struct x264_hrd_t
    {
        public double cpb_initial_arrival_time;

        public double cpb_final_arrival_time;

        public double cpb_removal_time;

        public double dpb_output_time;
    }

    public unsafe partial struct x264_sei_payload_t
    {
        public int payload_size;

        public int payload_type;

        [NativeTypeName("uint8_t *")]
        public byte* payload;
    }

    public unsafe partial struct x264_sei_t
    {
        public int num_payloads;

        public x264_sei_payload_t* payloads;

        [NativeTypeName("void (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> sei_free;
    }

    public unsafe partial struct x264_image_t
    {
        public X264Csp i_csp;

        public int i_plane;

        [NativeTypeName("int [4]")]
        public fixed int i_stride[4];

        [NativeTypeName("uint8_t *[4]")]
        public _plane_e__FixedBuffer plane;

        public unsafe partial struct _plane_e__FixedBuffer
        {
            public byte* e0;
            public byte* e1;
            public byte* e2;
            public byte* e3;

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

    public unsafe partial struct x264_image_properties_t
    {
        public float* quant_offsets;

        [NativeTypeName("void (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> quant_offsets_free;

        [NativeTypeName("uint8_t *")]
        public byte* mb_info;

        [NativeTypeName("void (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> mb_info_free;

        public double f_ssim;

        public double f_psnr_avg;

        [NativeTypeName("double [3]")]
        public fixed double f_psnr[3];

        public double f_crf_avg;
    }

    public unsafe partial struct x264_picture_t
    {
        public X264Type i_type;

        public int i_qpplus1;

        public int i_pic_struct;

        public int b_keyframe;

        [NativeTypeName("int64_t")]
        public long i_pts;

        [NativeTypeName("int64_t")]
        public long i_dts;

        public x264_param_t* param;

        public x264_image_t img;

        public x264_image_properties_t prop;

        public x264_hrd_t hrd_timing;

        public x264_sei_t extra_sei;

        public void* opaque;
    }

    public static unsafe partial class x264
    {
        [NativeTypeName("const char *const [5]")]
        public static readonly sbyte[][] x264_direct_pred_names = new sbyte[][]
        {
            new sbyte[] { 0x6E, 0x6F, 0x6E, 0x65, 0x00 },
            new sbyte[] { 0x73, 0x70, 0x61, 0x74, 0x69, 0x61, 0x6C, 0x00 },
            new sbyte[] { 0x74, 0x65, 0x6D, 0x70, 0x6F, 0x72, 0x61, 0x6C, 0x00 },
            new sbyte[] { 0x61, 0x75, 0x74, 0x6F, 0x00 },
        };

        [NativeTypeName("const char *const [6]")]
        public static readonly sbyte[][] x264_motion_est_names = new sbyte[][]
        {
            new sbyte[] { 0x64, 0x69, 0x61, 0x00 },
            new sbyte[] { 0x68, 0x65, 0x78, 0x00 },
            new sbyte[] { 0x75, 0x6D, 0x68, 0x00 },
            new sbyte[] { 0x65, 0x73, 0x61, 0x00 },
            new sbyte[] { 0x74, 0x65, 0x73, 0x61, 0x00 },
        };

        [NativeTypeName("const char *const [4]")]
        public static readonly sbyte[][] x264_b_pyramid_names = new sbyte[][]
        {
            new sbyte[] { 0x6E, 0x6F, 0x6E, 0x65, 0x00 },
            new sbyte[] { 0x73, 0x74, 0x72, 0x69, 0x63, 0x74, 0x00 },
            new sbyte[] { 0x6E, 0x6F, 0x72, 0x6D, 0x61, 0x6C, 0x00 },
        };

        [NativeTypeName("const char *const [4]")]
        public static readonly sbyte[][] x264_overscan_names = new sbyte[][]
        {
            new sbyte[] { 0x75, 0x6E, 0x64, 0x65, 0x66, 0x00 },
            new sbyte[] { 0x73, 0x68, 0x6F, 0x77, 0x00 },
            new sbyte[] { 0x63, 0x72, 0x6F, 0x70, 0x00 },
        };

        [NativeTypeName("const char *const [7]")]
        public static readonly sbyte[][] x264_vidformat_names = new sbyte[][]
        {
            new sbyte[] { 0x63, 0x6F, 0x6D, 0x70, 0x6F, 0x6E, 0x65, 0x6E, 0x74, 0x00 },
            new sbyte[] { 0x70, 0x61, 0x6C, 0x00 },
            new sbyte[] { 0x6E, 0x74, 0x73, 0x63, 0x00 },
            new sbyte[] { 0x73, 0x65, 0x63, 0x61, 0x6D, 0x00 },
            new sbyte[] { 0x6D, 0x61, 0x63, 0x00 },
            new sbyte[] { 0x75, 0x6E, 0x64, 0x65, 0x66, 0x00 },
        };

        [NativeTypeName("const char *const [3]")]
        public static readonly sbyte[][] x264_fullrange_names = new sbyte[][]
        {
            new sbyte[] { 0x6F, 0x66, 0x66, 0x00 },
            new sbyte[] { 0x6F, 0x6E, 0x00 },
        };

        [NativeTypeName("const char *const [14]")]
        public static readonly sbyte[][] x264_colorprim_names = new sbyte[][]
        {
            new sbyte[] { 0x00 },
            new sbyte[] { 0x62, 0x74, 0x37, 0x30, 0x39, 0x00 },
            new sbyte[] { 0x75, 0x6E, 0x64, 0x65, 0x66, 0x00 },
            new sbyte[] { 0x00 },
            new sbyte[] { 0x62, 0x74, 0x34, 0x37, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x34, 0x37, 0x30, 0x62, 0x67, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x31, 0x37, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x32, 0x34, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x66, 0x69, 0x6C, 0x6D, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x32, 0x30, 0x32, 0x30, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x34, 0x32, 0x38, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x34, 0x33, 0x31, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x34, 0x33, 0x32, 0x00 },
        };

        [NativeTypeName("const char *const [20]")]
        public static readonly sbyte[][] x264_transfer_names = new sbyte[][]
        {
            new sbyte[] { 0x00 },
            new sbyte[] { 0x62, 0x74, 0x37, 0x30, 0x39, 0x00 },
            new sbyte[] { 0x75, 0x6E, 0x64, 0x65, 0x66, 0x00 },
            new sbyte[] { 0x00 },
            new sbyte[] { 0x62, 0x74, 0x34, 0x37, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x34, 0x37, 0x30, 0x62, 0x67, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x31, 0x37, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x32, 0x34, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x6C, 0x69, 0x6E, 0x65, 0x61, 0x72, 0x00 },
            new sbyte[] { 0x6C, 0x6F, 0x67, 0x31, 0x30, 0x30, 0x00 },
            new sbyte[] { 0x6C, 0x6F, 0x67, 0x33, 0x31, 0x36, 0x00 },
            new sbyte[] { 0x69, 0x65, 0x63, 0x36, 0x31, 0x39, 0x36, 0x36, 0x2D, 0x32, 0x2D, 0x34, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x31, 0x33, 0x36, 0x31, 0x65, 0x00 },
            new sbyte[] { 0x69, 0x65, 0x63, 0x36, 0x31, 0x39, 0x36, 0x36, 0x2D, 0x32, 0x2D, 0x31, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x32, 0x30, 0x32, 0x30, 0x2D, 0x31, 0x30, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x32, 0x30, 0x32, 0x30, 0x2D, 0x31, 0x32, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x32, 0x30, 0x38, 0x34, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x34, 0x32, 0x38, 0x00 },
            new sbyte[] { 0x61, 0x72, 0x69, 0x62, 0x2D, 0x73, 0x74, 0x64, 0x2D, 0x62, 0x36, 0x37, 0x00 },
        };

        [NativeTypeName("const char *const [16]")]
        public static readonly sbyte[][] x264_colmatrix_names = new sbyte[][]
        {
            new sbyte[] { 0x47, 0x42, 0x52, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x37, 0x30, 0x39, 0x00 },
            new sbyte[] { 0x75, 0x6E, 0x64, 0x65, 0x66, 0x00 },
            new sbyte[] { 0x00 },
            new sbyte[] { 0x66, 0x63, 0x63, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x34, 0x37, 0x30, 0x62, 0x67, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x31, 0x37, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x32, 0x34, 0x30, 0x6D, 0x00 },
            new sbyte[] { 0x59, 0x43, 0x67, 0x43, 0x6F, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x32, 0x30, 0x32, 0x30, 0x6E, 0x63, 0x00 },
            new sbyte[] { 0x62, 0x74, 0x32, 0x30, 0x32, 0x30, 0x63, 0x00 },
            new sbyte[] { 0x73, 0x6D, 0x70, 0x74, 0x65, 0x32, 0x30, 0x38, 0x35, 0x00 },
            new sbyte[] { 0x63, 0x68, 0x72, 0x6F, 0x6D, 0x61, 0x2D, 0x64, 0x65, 0x72, 0x69, 0x76, 0x65, 0x64, 0x2D, 0x6E, 0x63, 0x00 },
            new sbyte[] { 0x63, 0x68, 0x72, 0x6F, 0x6D, 0x61, 0x2D, 0x64, 0x65, 0x72, 0x69, 0x76, 0x65, 0x64, 0x2D, 0x63, 0x00 },
            new sbyte[] { 0x49, 0x43, 0x74, 0x43, 0x70, 0x00 },
        };

        [NativeTypeName("const char *const [4]")]
        public static readonly sbyte[][] x264_nal_hrd_names = new sbyte[][]
        {
            new sbyte[] { 0x6E, 0x6F, 0x6E, 0x65, 0x00 },
            new sbyte[] { 0x76, 0x62, 0x72, 0x00 },
            new sbyte[] { 0x63, 0x62, 0x72, 0x00 },
        };

        [NativeTypeName("const char *const [3]")]
        public static readonly sbyte[][] x264_avcintra_flavor_names = new sbyte[][]
        {
            new sbyte[] { 0x70, 0x61, 0x6E, 0x61, 0x73, 0x6F, 0x6E, 0x69, 0x63, 0x00 },
            new sbyte[] { 0x73, 0x6F, 0x6E, 0x79, 0x00 },
        };

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_nal_encode(x264_t* h, [NativeTypeName("uint8_t *")] byte* dst, x264_nal_t* nal);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_param_default(x264_param_t* param0);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_param_parse(x264_param_t* param0, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("const char *")] sbyte* value);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_param_cleanup(x264_param_t* param0);

        [NativeTypeName("const char *const [11]")]
        public static readonly sbyte[][] x264_preset_names = new sbyte[][]
        {
            new sbyte[] { 0x75, 0x6C, 0x74, 0x72, 0x61, 0x66, 0x61, 0x73, 0x74, 0x00 },
            new sbyte[] { 0x73, 0x75, 0x70, 0x65, 0x72, 0x66, 0x61, 0x73, 0x74, 0x00 },
            new sbyte[] { 0x76, 0x65, 0x72, 0x79, 0x66, 0x61, 0x73, 0x74, 0x00 },
            new sbyte[] { 0x66, 0x61, 0x73, 0x74, 0x65, 0x72, 0x00 },
            new sbyte[] { 0x66, 0x61, 0x73, 0x74, 0x00 },
            new sbyte[] { 0x6D, 0x65, 0x64, 0x69, 0x75, 0x6D, 0x00 },
            new sbyte[] { 0x73, 0x6C, 0x6F, 0x77, 0x00 },
            new sbyte[] { 0x73, 0x6C, 0x6F, 0x77, 0x65, 0x72, 0x00 },
            new sbyte[] { 0x76, 0x65, 0x72, 0x79, 0x73, 0x6C, 0x6F, 0x77, 0x00 },
            new sbyte[] { 0x70, 0x6C, 0x61, 0x63, 0x65, 0x62, 0x6F, 0x00 },
        };

        [NativeTypeName("const char *const [9]")]
        public static readonly sbyte[][] x264_tune_names = new sbyte[][]
        {
            new sbyte[] { 0x66, 0x69, 0x6C, 0x6D, 0x00 },
            new sbyte[] { 0x61, 0x6E, 0x69, 0x6D, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x00 },
            new sbyte[] { 0x67, 0x72, 0x61, 0x69, 0x6E, 0x00 },
            new sbyte[] { 0x73, 0x74, 0x69, 0x6C, 0x6C, 0x69, 0x6D, 0x61, 0x67, 0x65, 0x00 },
            new sbyte[] { 0x70, 0x73, 0x6E, 0x72, 0x00 },
            new sbyte[] { 0x73, 0x73, 0x69, 0x6D, 0x00 },
            new sbyte[] { 0x66, 0x61, 0x73, 0x74, 0x64, 0x65, 0x63, 0x6F, 0x64, 0x65, 0x00 },
            new sbyte[] { 0x7A, 0x65, 0x72, 0x6F, 0x6C, 0x61, 0x74, 0x65, 0x6E, 0x63, 0x79, 0x00 },
        };

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_param_default_preset(x264_param_t* param0, [NativeTypeName("const char *")] sbyte* preset, [NativeTypeName("const char *")] sbyte* tune);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_param_apply_fastfirstpass(x264_param_t* param0);

        [NativeTypeName("const char *const [7]")]
        public static readonly sbyte[][] x264_profile_names = new sbyte[][]
        {
            new sbyte[] { 0x62, 0x61, 0x73, 0x65, 0x6C, 0x69, 0x6E, 0x65, 0x00 },
            new sbyte[] { 0x6D, 0x61, 0x69, 0x6E, 0x00 },
            new sbyte[] { 0x68, 0x69, 0x67, 0x68, 0x00 },
            new sbyte[] { 0x68, 0x69, 0x67, 0x68, 0x31, 0x30, 0x00 },
            new sbyte[] { 0x68, 0x69, 0x67, 0x68, 0x34, 0x32, 0x32, 0x00 },
            new sbyte[] { 0x68, 0x69, 0x67, 0x68, 0x34, 0x34, 0x34, 0x00 },
        };

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_param_apply_profile(x264_param_t* param0, [NativeTypeName("const char *")] sbyte* profile);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_picture_init(x264_picture_t* pic);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_picture_alloc(x264_picture_t* pic, X264Csp i_csp, int i_width, int i_height);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_picture_clean(x264_picture_t* pic);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern x264_t* x264_encoder_open_163(x264_param_t* param0);
        public static x264_t* x264_encoder_open(x264_param_t* param0) => x264_encoder_open_163(param0);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_encoder_reconfig(x264_t* param0, x264_param_t* param1);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_encoder_parameters(x264_t* param0, x264_param_t* param1);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_encoder_headers(x264_t* param0, x264_nal_t** pp_nal, int* pi_nal);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_encoder_encode(x264_t* param0, x264_nal_t** pp_nal, int* pi_nal, x264_picture_t* pic_in, x264_picture_t* pic_out);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_encoder_close(x264_t* param0);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_encoder_delayed_frames(x264_t* param0);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_encoder_maximum_delayed_frames(x264_t* param0);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void x264_encoder_intra_refresh(x264_t* param0);

        [DllImport("x264", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int x264_encoder_invalidate_reference(x264_t* param0, [NativeTypeName("int64_t")] long pts);
    }
}
