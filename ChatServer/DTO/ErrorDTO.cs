namespace ChatServer.DTO
{
    public class ErrorDTO
    {
        public ErrorDTO(int c, string m)
        {
            code = c;
            message = m;
        }

        public int code { get; set; }

        public string message { get; set; }
    }
}
