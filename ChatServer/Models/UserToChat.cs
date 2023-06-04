using Microsoft.EntityFrameworkCore;

namespace ChatServer.Models
{
    public class UserToChat
    {
        public int id_connection { get; set; }

        public int rk_id_user { get; set; }

        public int rk_id_chat { get; set; }

        public virtual Chat RkIdChatNavigation { get; set; } = null!;

        public virtual User RkIdUserNavigation { get; set; } = null!;
    }
}
