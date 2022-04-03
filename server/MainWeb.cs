using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OptimeGBAServer.Media;
using OptimeGBAServer.Services;

namespace OptimeGBAServer
{
    public class MainWeb
    {
        private static void InjectDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<GbaHostService>();
            builder.Services.AddHostedService<GbaHostService>(s => s.GetRequiredService<GbaHostService>());

            builder.Services.AddSingleton<VideoSubjectService>();
            builder.Services.AddHostedService<VideoSubjectService>(s => s.GetRequiredService<VideoSubjectService>());

            builder.Services.AddSingleton<AudioSubjectService>();
            builder.Services.AddHostedService<AudioSubjectService>(s => s.GetRequiredService<AudioSubjectService>());

            switch (builder.Configuration["VideoEncoding"].ToString()) {
                case "vp9":
                    builder.Services.AddSingleton<IGbaRenderer, Vp9RendererService>();
                    break;

                case "h264":
                    builder.Services.AddSingleton<IGbaRenderer, H264RendererService>();
                    break;

                case "h264highres":
                    builder.Services.AddSingleton<IGbaRenderer, H264HighResRendererService>();
                    break;

                default:
                    throw new ArgumentException("VideoEncoding must be either \"vp9\" or \"h264\".");
            }
            builder.Services.AddHostedService<IGbaRenderer>(s => s.GetRequiredService<IGbaRenderer>());

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

            if (libraryName == "openh264")
            {
                if (OperatingSystem.IsLinux())
                {
                    // libopenh264 2.2.0
                    return NativeLibrary.Load("libopenh264-2.2.0-linux-arm64.6.so", assembly, searchPath);
                }
                else if (OperatingSystem.IsWindows())
                {
                    // libopenh264 2.2.0
                    return NativeLibrary.Load("openh264-2.2.0-win64.dll", assembly, searchPath);
                }
            }

            // Otherwise, fallback to default import resolver.
            return IntPtr.Zero;
        }
    }
}
