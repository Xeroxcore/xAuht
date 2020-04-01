using System;

namespace xAuth.Interface
{
    public interface ILockout
    {
        int Id { get; set; }
        int LockOut { get; set; }
        DateTime LockExpire { get; set; }
    }
}