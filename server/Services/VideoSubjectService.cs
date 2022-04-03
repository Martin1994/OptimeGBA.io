using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Models;

namespace OptimeGBAServer.Services
{
    public struct VideoSubjectPayload
    {
        public ReadOnlyMemory<byte> Buffer { get; set; }
        public FrameMetadata FrameMetadata { get; set; }
    }

    public class VideoSubjectService : SubjectService<VideoSubjectPayload>
    {
        public VideoSubjectService(IHostApplicationLifetime lifetime, ILogger<VideoSubjectService> logger) : base(lifetime, logger) { }
    }
}
