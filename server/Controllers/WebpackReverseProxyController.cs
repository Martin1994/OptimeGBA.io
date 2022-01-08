using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace OptimeGBAServer.Controllers
{
    [Route("/scripts/{requestedPath}")]
    public class WebpackReverseProxyController : ControllerBase
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string _uriPrefix;

        public WebpackReverseProxyController(IConfiguration configuration)
        {
            string webpackPrefix = configuration["WebpackAddress"] ?? "http://127.0.0.1:5001";
            _uriPrefix = webpackPrefix.Trim('/') + "/";
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? requestedPath, CancellationToken cancellationToken)
        {
            if (requestedPath is null)
            {
                return new StatusCodeResult(404);
            }

            try
            {
                return new FileStreamResult(await _client.GetStreamAsync(_uriPrefix + requestedPath, cancellationToken), "text/javascript");
            }
            catch (HttpRequestException ex)
            {
                return new ContentResult()
                {
                    Content = ex.Message,
                    StatusCode = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError)
                };
            }
        }
    }
}
