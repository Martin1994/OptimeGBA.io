namespace OptimeGBAServer.Models
{
    public class ConsoleInterfaceResponse
    {
        public string? Action { get; set; }

        public InitAction? InitAction { get; set; }

        public FrameAction? FrameAction { get; set; }
        public PongAction? PongAction { get; set; }
    }
}
