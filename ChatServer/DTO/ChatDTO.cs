namespace ChatServer.DTO
{
    public class ChatDTO
    {
        public int id_chat { get; set; }

        public string? name_chat { get; set; }

        public int rk_type_chat { get; set; }

        public string link { get; set; } = null!;
    }
}
