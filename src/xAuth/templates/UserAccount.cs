using System.ComponentModel.DataAnnotations;
using xAuth.Interface;

namespace xAuth
{
    public class UserAccount : Lockout, IUser
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}