using System;

namespace xAuth.Interface
{
    public interface IUser
    {
        string UserName { get; set; }
        string Password { get; set; }
    }
}