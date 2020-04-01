using System;
using Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xAuth.Interface;
using xSql;

namespace xAuth.test
{
    [TestClass]
    public class AuthenticationUserTest
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
            Assert.IsTrue(token.Token.Length > 10);
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
            IUser user = new UserAccount()
            {
                UserName = "Nasar2",
                Password = "helloworld2",
            };
            var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
            sql.AlterDataQuery("update useraccount set lockout = 2, lockexpire = now() Where username = @UserName", user);
            try
            {
                var token = Authentication.AuthentiacteUser(user, "user", "localhost");
            }
            catch
            {
                var table = sql.SelectQuery("select * from getuser(@UserName)", user);
                var dbUser = ObjectConverter.ConvertDataTableRowToObject<UserAccount>(table, 0);
                Assert.AreEqual(3, dbUser.LockOut);
            }
        }

        [TestMethod]
        public void UnLockAccount()
        {
            IUser user = new UserAccount()
            {
                UserName = "Nasar2",
                Password = "helloworld",
                LockExpire = DateTime.Now.AddMinutes(-30)
            };
            var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
            sql.AlterDataQuery("update useraccount set lockout = 3, lockexpire = @LockExpire Where username = @UserName", user);
            var token = Authentication.AuthentiacteUser(user, "user", "localhost");
            Assert.IsTrue(token.Token.Length > 10);
        }
    }
}