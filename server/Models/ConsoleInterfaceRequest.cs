namespace OptimeGBAServer.Models
{
    public class ConsoleInterfaceRequest
    {
        public string? Action { get; set; }

        public KeyAction? KeyAction { get; set; }

        public FillTokenAction? FillTokenAction { get; set; }

        public PingAction? PingAction { get; set; }
    }
}
