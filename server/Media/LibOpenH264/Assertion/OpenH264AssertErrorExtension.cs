using OptimeGBAServer.Media.LibOpenH264.Native;

namespace OptimeGBAServer.Media.LibOpenH264.Assertion
{
    public static class OpenH264AssertErrorExtension
    {
        public static int OpenH264AssertError(this int ret)
        {
            if (ret < 0)
            {
                throw new OpenH264Exception((CM_RETURN)ret);
            }

            return ret;
        }
    }
}