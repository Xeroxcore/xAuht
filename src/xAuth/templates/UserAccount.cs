using xAuth.Interface;

namespace xAuth
{
    public class UserAccount : Lockout, IUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}