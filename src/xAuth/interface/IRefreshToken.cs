using System;

namespace xAuth.Interface
{
    public interface IRefreshToken
    {

        int Id { get; set; }
        string Token { get; set; }
        DateTime Expires { get; set; }
        int UserAccount_Id { get; set; }
    }
}