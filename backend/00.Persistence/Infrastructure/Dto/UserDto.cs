using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Dto
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public string? Base64Image { get; set; }
    }
}
