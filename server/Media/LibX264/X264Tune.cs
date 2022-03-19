using static OptimeGBAServer.Media.LibX264.Native.x264;

namespace OptimeGBAServer.Media.LibX264
{
    public enum X264Tune
    {
        None,
        Film,
        Animation,
        Grain,
        Stillimage,
        Psnr,
        Ssim,
        Fastdecode,
        Zerolatency
    }

    public static class X264TuneExtension {
        public static sbyte[]? ToCString(this X264Tune tune)
        {
            if (tune == X264Tune.None)
            {
                return null;
            }

            return x264_tune_names[(int)tune - 1];
        }
    }
}
