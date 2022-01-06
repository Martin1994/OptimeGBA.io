using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OptimeGBAServer.Utilities;

namespace OptimeGBAServer
{
    public class MainWeb
    {
        private static void InjectDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<GbaHostService>();
            builder.Services.AddHostedService<GbaHostService>(s => s.GetRequiredService<GbaHostService>());

            builder.Services.AddSingleton<ScreenSubjectService>();
            builder.Services.AddHostedService<ScreenSubjectService>(s => s.GetRequiredService<ScreenSubjectService>());

            builder.Services.AddSingleton<ScreenshotHelper>();

            builder.Services.AddControllers();
        }

        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            InjectDependencies(builder);

            WebApplication app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseWebSockets();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
