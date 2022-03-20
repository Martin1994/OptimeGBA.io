using System;
using OptimeGBAServer.Media.LibOpenH264.Native;

namespace OptimeGBAServer.Media.LibOpenH264
{
    public class OpenH264Exception : Exception
    {
        public CM_RETURN Code { get; init; }

        public OpenH264Exception(CM_RETURN code)
        {
            Code = code;
        }
    }
}
