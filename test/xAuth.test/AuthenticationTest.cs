using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xAuth.Interface;
using xSql;

namespace xAuth.test
{
    [TestClass]
    public class AuthenticationTest
    {
        private readonly IAuth Authentication = new Auth(
            new NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld"),
            new JwtGenerator("asdas1d31q51131#", "HS256"));

        [TestMethod]
        public void AuthenticateUser()
        {
            IUser user = new UserAccount()
            {
                UserName = "Nasar",
                Password = "helloworld"
            };
            var token = Authentication.AuthentiacteUser(user, "user", "localhost");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token.Token));
        }

        [TestMethod]
        public void AuthenticateTokenKey()
        {
            IToken user = new TokenKey()
            {
                Token = "helloworld123key"
            };
            var token = Authentication.AuthenticateTokenKey(user, "tokenkey-", "localhost");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token.Token));
        }

        [TestMethod]
        public void AccountIsBlocked()
        {
            try
            {

                IUser user = new UserAccount()
                {
                    UserName = "Nasar2",
                    Password = "helloworld",
                    LockOut = 3,
                };
                var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
                sql.AlterDataQuery("update useraccount set lockout = @LockOut, lockexpire = now() Where username = @UserName", user);
                var token = Authentication.AuthentiacteUser(user, "user", "localhost");
            }
            catch (Exception error)
            {
                Assert.AreEqual("Account has been locked please try again later", error.Message);
            }
        }

        [TestMethod]
        public void LockAccount()
        {
            try
            {
                IUser user = new UserAccount()
                {
                    UserName = "Nasar2",
                    Password = "helloworld2",
                    LockOut = 2,
                };
                var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
                sql.AlterDataQuery("update useraccount set lockout = @LockOut, lockexpire = now() Where username = @UserName", user);
                var token = Authentication.AuthentiacteUser(user, "user", "localhost");
            }
            catch (Exception error)
            {
                Assert.AreEqual("", error.Message);
            }
        }
    }
}