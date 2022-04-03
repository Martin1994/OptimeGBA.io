namespace OptimeGBAServer.Models
{
    public interface ConsoleInterfaceResponse
    {
        public string Action { get; }
    }

    public struct InitResponse : ConsoleInterfaceResponse
    {
        public string Action { get => "init"; }
        public string? Codec { get; init; }
    }

    public struct PongResponse : ConsoleInterfaceResponse
    {
        public string Action { get => "pong"; }
        public double MadeAt { get; init; }
    }
}
