using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OptimeGBAServer.Media;

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
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            InjectDependencies(builder);

            WebApplication app = builder.Build();

            app.UseWebSockets();

            app.MapControllers();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            await app.RunAsync();
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == "vpx")
            {
                if (OperatingSystem.IsLinux())
                {
                    // libvpx 1.11
                    return NativeLibrary.Load("libvpx.so.7", assembly, searchPath);
                }
            }

            // Otherwise, fallback to default import resolver.
            return IntPtr.Zero;
        }
    }
}
