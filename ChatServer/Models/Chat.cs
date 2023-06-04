using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ChatServer.Models
{
    public class Chat
    {
        [Key]
        public int id_chat { get; set; }

        public string? name_chat { get; set; }

        public int rk_type_chat { get; set; }

        public string link { get; set; } = null!;

        public virtual ICollection<Message> Messages { get;} = new List<Message>();

        public virtual TypeChat RkTypeChatNavigation { get; set; } = null!;

        public virtual ICollection<UserToChat> Chat_UserToChats { get; } = new List<UserToChat>();
    }
}
