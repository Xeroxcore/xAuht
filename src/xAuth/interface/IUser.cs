using System;

namespace xAuth.Interface
{
    public interface IUser
    {
        int Id { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int LockOut { get; set; }
        DateTime LockExpire { get; set; }
    }
}