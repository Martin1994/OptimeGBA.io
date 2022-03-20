using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OptimeGBAServer.Models;
using OptimeGBAServer.Services;

namespace OptimeGBAServer.Controllers
{
    [ApiController]
    [Route("/status.json")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly GbaHostService _gba;
        private readonly IGbaRenderer _renderer;
        private readonly ScreenSubjectService _screen;

        public StatusController(ILogger<StatusController> logger, GbaHostService gba, IGbaRenderer renderer, ScreenSubjectService screen)
        {
            _logger = logger;
            _gba = gba;
            _renderer = renderer;
            _screen = screen;
        }

        [HttpGet]
        public OptimeStatus Get()
        {
            return new OptimeStatus()
            {
                Fps = _gba.Fps,
                Bps = _renderer.Bpf * _gba.Fps,
                UpTime = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds,
                Connections = _screen.ObserverCount
            };
        }
    }
}
