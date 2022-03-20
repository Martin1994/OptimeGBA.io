using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Models;

namespace OptimeGBAServer.Services
{
    public struct ScreenSubjectPayload
    {
        public ReadOnlyMemory<byte> Buffer { get; set; }
        public FrameMetadata FrameMetadata { get; set; }
    }
    public class ScreenSubjectService : SubjectService<ScreenSubjectPayload>
    {
        public ScreenSubjectService(IHostApplicationLifetime lifetime, ILogger<ScreenSubjectService> logger) : base(lifetime, logger) { }
    }
}
