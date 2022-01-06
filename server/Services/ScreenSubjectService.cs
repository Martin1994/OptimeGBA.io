using Microsoft.Extensions.Logging;

namespace OptimeGBAServer
{
    public class ScreenSubjectService : AbstractSubjectService
    {
        public ScreenSubjectService(ILogger<ScreenSubjectService> logger) : base(logger) { }
    }
}
