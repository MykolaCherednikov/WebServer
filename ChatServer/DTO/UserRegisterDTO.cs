using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ChatServer.DTO
{
    public class UserRegisterDTO
    {
        [Required]
        public string nickname { get; set; } = null!;
        [Required]
        public string email { get; set; } = null!;
        [Required]
        public string password { get; set; } = null!;

    }
}
