using System;
using System.Collections.Generic;
using System.Security.Claims;
using Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xAuth.Interface;
using xSql;

namespace xAuth.test
{
    [TestClass]
    public class AuthenticationUserTest
    {
        private readonly IAuth Authentication = new UserAuth(
            new NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld"),
            new JwtGenerator("asdas1d31q51131#", "HS256"));

        private readonly NpgSql Sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");

        [TestMethod]
        public void AuthenticateUser()
        {
            IUser user = new UserAccount()
            {
                UserName = "Nasar",
                Password = "helloworld"
            };
            var token = Authentication.Authentiacte(user, "user", "localhost", AddRole);
            Assert.IsTrue(token.Token.Length > 10);
        }

        public List<Claim> AddRole(int id)
        {
            var claimList = new List<Claim>();
            var claim = new Claim("Role", "Admin");
            claimList.Add(claim);
            return claimList;
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
                Sql.AlterDataQuery("update useraccount set lockout = @LockOut, lockexpire = now() Where username = @UserName", user);
                var token = Authentication.Authentiacte(user, "user", "localhost", null);
                Assert.AreEqual("", token.Token);
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
                Password = "asd",
            };
            Sql.AlterDataQuery("update useraccount set lockout = 2, lockexpire = now() Where username = @UserName", user);
            try
            {
                var token = Authentication.Authentiacte(user, "user", "localhost", null);
                Assert.IsFalse(token.Token.Length > 10);
            }
            catch
            {
                var table = Sql.SelectQuery("select * from getuser(@UserName)", user);
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
            Sql.AlterDataQuery("update useraccount set lockout = 3, lockexpire = @LockExpire Where username = @UserName", user);
            var token = Authentication.Authentiacte(user, "user", "localhost", null);
            Assert.IsTrue(token.Token.Length > 10);
        }

        [TestMethod]
        public void ReauthenticateWithRefreshUser()
        {
            try
            {
                UserAccount user = new UserAccount()
                {
                    UserName = "Nasar2",
                    Password = "helloworld",
                };
                var auth = Authentication.Authentiacte(user, "user", "localhost", null);
                var table = Sql.SelectQuery("select * from getrefreshtoken(@RefreshToken)", auth);
                var result = Authentication.RefreshToken(auth.RefreshToken, "user", "localhost", null);
                Assert.IsTrue(result.Token.Length > 3);
            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        public void TestUsedRefreshToken()
        {
            try
            {
                UserAccount user = new UserAccount()
                {
                    UserName = "Nasar2",
                    Password = "helloworld",
                };
                var auth = Authentication.Authentiacte(user, "user", "localhost", null);
                var table = Sql.SelectQuery("select * from getrefreshtoken(@RefreshToken)", auth);
                var LockedRefToken = new RefreshToken() { Token = auth.RefreshToken };
                Sql.AlterDataQuery("update refreshtoken set used = true where token = @Token", LockedRefToken);
                var result = Authentication.RefreshToken(auth.RefreshToken, "user", "localhost", null);
                Assert.AreEqual("", result.Token);
            }
            catch (Exception error)
            {
                Assert.AreEqual("Warning: The Refreshtoken has already been used", error.Message);
            }
        }
    }
}