using static OptimeGBAServer.Media.LibX264.Native.x264;

namespace OptimeGBAServer.Media.LibX264
{
    public enum X264Preset
    {
        None,
        Ultrafast,
        Superfast,
        Veryfast,
        Faster,
        Fast,
        Medium,
        Slow,
        Slower,
        Veryslow,
        Placebo
    }

    public static class X264PresetExtension
    {
        public static sbyte[]? ToCString(this X264Preset preset)
        {
            if (preset == X264Preset.None)
            {
                return null;
            }

            return x264_preset_names[(int)preset - 1];
        }
    }
}
