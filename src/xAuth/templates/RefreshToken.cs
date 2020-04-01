using System;
using xAuth.Interface;

namespace xAuth
{
    public class RefreshToken : IRefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expired { get; set; }
        public bool Used { get; set; }
        public int UserId { get; set; }
        public int TokenId { get; set; }
    }
}