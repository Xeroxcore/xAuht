using xAuth.Interface;

namespace xAuth
{
    public class TokenKey : Lockout, IToken
    {
        public string Token { get; set; }
    }
}