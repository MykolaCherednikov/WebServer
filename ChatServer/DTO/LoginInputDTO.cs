using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace ChatServer.DTO
{
    public class LoginInputDTO
    {
        [Required]
        public string email { get; set; } = null!;
        [Required]
        public string password { get; set; } = null!;
    }
}
