using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OptimeGBA;

namespace OptimeGBAServer.Services
{
    public interface IGbaRenderer : IHostedService
    {
        /// <summary>
        /// Codec string from WebCodecs Codec Registry.
        /// </summary>
        /// <see href="link">https://www.w3.org/TR/webcodecs-codec-registry/</see>
        public string CodecString { get; }

        public double Bpf { get; }

        public ValueTask EnqueueFrame(Gba gba, CancellationToken cancellationToken);
    }
}
