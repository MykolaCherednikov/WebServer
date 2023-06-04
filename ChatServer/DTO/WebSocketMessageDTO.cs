namespace ChatServer.DTO
{
    public enum WebSocketDataType
    {
        SendMessage = 0,
        EditMessage = 1,
        DeleteMessage = 2,
        CreateChat = 3,
        EditChat = 4,
        DeleteChat = 5,
        Token = 6
    }

    public class WebSocketMessageDTO
    {
        public string? data { get; set; }
    }
}
