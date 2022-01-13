using System;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Models;

namespace OptimeGBAServer
{
    public struct ScreenSubjectPayload {
        public ReadOnlyMemory<byte> Buffer { get; set; }
        public FrameMetadata FrameMetadata { get; set; }
    }
    public class ScreenSubjectService : AbstractSubjectService<ScreenSubjectPayload>
    {
        public ScreenSubjectService(ILogger<ScreenSubjectService> logger) : base(logger) { }
    }
}
