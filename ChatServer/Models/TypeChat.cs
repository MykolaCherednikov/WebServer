using System.ComponentModel.DataAnnotations;

namespace ChatServer.Models
{
    public class TypeChat
    {
        public int id_chat_type { get; set; }

        public string name_chat_type { get; set; } = null!;

        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
