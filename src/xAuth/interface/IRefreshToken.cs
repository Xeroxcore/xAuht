using System;

namespace xAuth.Interface
{
    public interface IRefreshToken
    {
        int Id { get; set; }
        string Token { get; set; }
        DateTime Expires { get; set; }
        bool Used { get; set; }
        int UserId { get; set; }
        int TokenId { get; set; }
    }
}