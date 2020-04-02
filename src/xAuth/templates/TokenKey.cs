using System.ComponentModel.DataAnnotations;
using xAuth.Interface;

namespace xAuth
{
    public class TokenKey : Lockout, IToken
    {
        [Required]
        public string Token { get; set; }
    }
}