using System;
using OptimeGBAServer.Media.LibVpx.Native;

using static OptimeGBAServer.Media.LibVpx.Native.vpx_codec_cx_pkt_kind;

namespace OptimeGBAServer.Media.LibVpx
{
    public unsafe struct VpxPacket
    {
        public vpx_codec_cx_pkt_t* Raw { get; }

        public vpx_codec_cx_pkt_kind Kind => Raw->kind;

        public VpxPacket(vpx_codec_cx_pkt_t* packet)
        {
            Raw = packet;
        }

        public FrameData DataAsFrame
        {
            get
            {
                if (Kind != VPX_CODEC_CX_FRAME_PKT)
                {
                    throw new VpxException($"Data type must be {VPX_CODEC_CX_FRAME_PKT}");
                }
                return new FrameData(&Raw->data.frame);
            }
        }

        public PsnrData DataAsPsnr
        {
            get
            {
                if (Kind != VPX_CODEC_PSNR_PKT)
                {
                    throw new VpxException($"Data type must be {VPX_CODEC_PSNR_PKT}");
                }
                return new PsnrData(&Raw->data.psnr);
            }
        }

        public Span<byte> DataAsFirstpassStats
        {
            get
            {
                if (Kind != VPX_CODEC_FPMB_STATS_PKT)
                {
                    throw new VpxException($"Data type must be {VPX_CODEC_FPMB_STATS_PKT}");
                }
                return new Span<byte>(Raw->data.firstpass_mb_stats.buf, (int)Raw->data.firstpass_mb_stats.sz);
            }
        }

        public Span<byte> DataAsTwopassStats
        {
            get
            {
                if (Kind != VPX_CODEC_STATS_PKT)
                {
                    throw new VpxException($"Data type must be {VPX_CODEC_STATS_PKT}");
                }
                return new Span<byte>(Raw->data.twopass_stats.buf, (int)Raw->data.twopass_stats.sz);
            }
        }

        public Span<byte> DataAsCustom
        {
            get
            {
                if (Kind != VPX_CODEC_CUSTOM_PKT)
                {
                    throw new VpxException($"Data type must be {VPX_CODEC_CUSTOM_PKT}");
                }
                return new Span<byte>(Raw->data.raw.buf, (int)Raw->data.raw.sz);
            }
        }

        public struct FrameData
        {
            public vpx_codec_cx_pkt_t.frame_t* Raw { get; }

            public FrameData(vpx_codec_cx_pkt_t.frame_t* frame)
            {
                if (frame == null)
                {
                    throw new VpxException("Frame pointer cannot be null.");
                }
                Raw = frame;
            }

            public Span<byte> Buf => new Span<byte>(Raw->buf, (int)Raw->sz);
            public vpx_codec_pts_t Pts => Raw->pts;
            public uint Duration => Raw->duration;
            public vpx_codec_frame_flags_t Flags => Raw->flags;
            public int PartitionId => Raw->partition_id;
            public Span<uint> Width => new Span<uint>(Raw->width, vpx_encoder.VPX_SS_MAX_LAYERS);
            public Span<uint> Height => new Span<uint>(Raw->height, vpx_encoder.VPX_SS_MAX_LAYERS);
            public Span<byte> SpatialLayerEncoded => new Span<byte>(Raw->spatial_layer_encoded, vpx_encoder.VPX_SS_MAX_LAYERS);
        }

        public struct PsnrData
        {
            public vpx_codec_cx_pkt_t.vpx_psnr_pkt_t* Raw { get; }

            public PsnrData(vpx_codec_cx_pkt_t.vpx_psnr_pkt_t* psnr)
            {
                if (psnr == null)
                {
                    throw new VpxException("Psnr pointer cannot be null.");
                }
                Raw = psnr;
            }

            public Span<uint> Samples => new Span<uint>(Raw->samples, vpx_encoder.VPX_SS_MAX_LAYERS);
            public Span<ulong> Sse => new Span<ulong>(Raw->sse, vpx_encoder.VPX_SS_MAX_LAYERS);
            public Span<double> Psnr => new Span<double>(Raw->psnr, vpx_encoder.VPX_SS_MAX_LAYERS);
        }
    }
}
