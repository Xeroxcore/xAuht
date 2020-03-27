using System;
using xAuth.Interface;

namespace xAuth
{
    public class TokenRespons : ITokenRespons
    {
        public string Token { get; set; }
        public string TokenType { get; set; }
        public string Expiration { get; set; }
        public string refreshToken { get; set; }
    }
}