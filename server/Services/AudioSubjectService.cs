using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptimeGBAServer.Services
{
    public struct AudioSubjectPayload
    {
        public ReadOnlyMemory<byte> Buffer { get; set; }
    }

    public class AudioSubjectService : SubjectService<AudioSubjectPayload>
    {
        public AudioSubjectService(IHostApplicationLifetime lifetime, ILogger<VideoSubjectService> logger) : base(lifetime, logger) { }
    }
}
