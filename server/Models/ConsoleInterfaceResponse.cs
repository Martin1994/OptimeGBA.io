namespace OptimeGBAServer.Models
{
    file interface IConsoleInterfaceResponse
    {
        public string Action { get; }
    }

    public readonly struct InitResponse : IConsoleInterfaceResponse
    {
        public string Action { get => "init"; }
        public string? Codec { get; init; }
    }

    public readonly struct PongResponse : IConsoleInterfaceResponse
    {
        public string Action { get => "pong"; }
        public double MadeAt { get; init; }
    }
}
