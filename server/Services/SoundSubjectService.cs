using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptimeGBAServer.Services
{
    public struct SoundSubjectPayload
    {
        public ReadOnlyMemory<byte> Buffer { get; set; }
    }
    public class SoundSubjectService : SubjectService<SoundSubjectPayload>
    {
        public SoundSubjectService(IHostApplicationLifetime lifetime, ILogger<ScreenSubjectService> logger) : base(lifetime, logger) { }
    }
}
