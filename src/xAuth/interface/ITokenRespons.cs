using System;

namespace xAuth.Interface
{
    public interface ITokenRespons
    {
        string Token { get; set; }
        string TokenType { get; set; }
        string Expiration { get; set; }
        string refreshToken { get; set; }
    }
}