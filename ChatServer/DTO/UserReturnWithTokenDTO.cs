namespace ChatServer.DTO
{
    public class UserReturnWithTokenDTO
    {
        public string? token { get; set; }

        public int id_user { get; set; }

        public string nickname { get; set; } = null!;

        public string email { get; set; } = null!;
    }
}
