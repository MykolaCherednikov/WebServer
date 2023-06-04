namespace ChatServer.DTO
{
    public class UserReturnDTO
    {
        public int id_user { get; set; }

        public string nickname { get; set; } = null!;

        public string email { get; set; } = null!;
    }
}
