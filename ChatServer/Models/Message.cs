using System.ComponentModel.DataAnnotations;

namespace ChatServer.Models
{
    public class Message
    {
        [Key]
        public int id_message { get; set; }

        public string? text_message { get; set; }

        public DateTime? data_time { get; set; }

        public int? rk_user { get; set; }

        public int? rk_chat { get; set; }

        public virtual Chat? RkChatNavigation { get; set; }

        public virtual User? RkUserNavigation { get; set; }
    }
}
