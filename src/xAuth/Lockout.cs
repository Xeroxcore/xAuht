using System;
using xAuth.Interface;

namespace xAuth
{
    public class Lockout : ILockout
    {
        public int Id { get; set; }
        public int LockOut { get; set; }
        public DateTime LockExpire { get; set; }
    }
}