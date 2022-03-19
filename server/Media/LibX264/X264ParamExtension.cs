using OptimeGBAServer.Media.LibX264.CheckError;
using OptimeGBAServer.Media.LibX264.Native;
using static OptimeGBAServer.Media.LibX264.Native.x264;

namespace OptimeGBAServer.Media.LibX264
{
    public static class X264ParamExtension
    {
        public static unsafe void ApplyFastfirstpass(ref this x264_param_t config)
        {
            fixed (x264_param_t* configPtr = &config)
            {
                x264_param_apply_fastfirstpass(configPtr);
            }
        }

        public static unsafe void ApplyProfile(ref this x264_param_t config, X264Profile profile)
        {
            fixed (x264_param_t* configPtr = &config)
            {
                fixed (sbyte* profileName = x264_profile_names[(int)profile])
                {
                    x264_param_apply_profile(configPtr, profileName).X264CheckError();
                }
            }
        }
    }

    public enum X264Profile
    {
        Baseline,
        Main,
        High,
        High10,
        High422,
        High444,
    }
}
