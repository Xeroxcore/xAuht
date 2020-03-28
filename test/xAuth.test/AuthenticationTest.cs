using Microsoft.VisualStudio.TestTools.UnitTesting;
using xAuth.Interface;

namespace xAuth.test
{
    [TestClass]
    public class AuthenticationTest
    {
        private IAuth Authentication = new Auth();

        [TestMethod]
        public void AuthenticateUser()
        {
            IUser user = new UserAccount()
            {
                UserName = "Nasar",
                Password = "helloworld"
            };
            var token = Authentication.AuthentiacteUser(user);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token.Token));
        }

        [TestMethod]
        public void AuthenticateTokenKey()
        {
            IToken user = new TokenKey()
            {
                Token = "helloworld123key"
            };
            var token = Authentication.AuthenticateTokenKey(user);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token.Token));
        }
    }
}