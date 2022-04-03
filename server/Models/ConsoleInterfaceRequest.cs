namespace OptimeGBAServer.Models
{
    public struct AudioControlRequest
    {
        public bool Mute { get; set; }
    }

    public struct DummyRequest
    {
    }

    public struct KeyRequest
    {
        public string? Key { get; set; }
        public string? Action { get; set; }
    }

    public struct PingRequest
    {
        public double MadeAt { get; set; }
    }
}
