using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ChatServer.Models
{
    public class User
    {
        [Key]
        public int id_user { get; set; }
        [Required]
        public string nickname { get; set; } = null!;
        [Required]
        public string email { get; set; } = null!;
        [Required]
        public string password { get; set; } = null!;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        public virtual ICollection<UserToChat> User_UserToChats { get; set; } = new List<UserToChat>();
    }
}
