using System;
using Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xAuth.Interface;
using xSql;

namespace xAuth.test
{
    [TestClass]
    public class AuthenticationTokenTest
    {
        private readonly IAuth Authentication = new TokenAuth(
            new NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld"),
            new JwtGenerator("asdas1d31q51131#", "HS256"));
        private readonly NpgSql Sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");

        [TestMethod]
        public void AuthenticateTokenKey()
        {
            IToken user = new TokenKey()
            {
                Token = "helloworld123key"
            };
            var result = Authentication.Authentiacte(user, "tokenkey-", "localhost");
            Assert.IsTrue(result.Token.Length > 10);
        }

        [TestMethod]
        public void AccountIsBlocked()
        {
            try
            {
                IToken token = new TokenKey()
                {
                    Token = "helloworld123key",
                    LockOut = 3,
                };
                Sql.AlterDataQuery("update tokenkey set lockout = @LockOut, lockexpire = now() Where token = @Token", token);
                var result = Authentication.Authentiacte(token, "user", "localhost");
                Assert.IsTrue(result.Token.Length > 3);
            }
            catch (Exception error)
            {
                Assert.AreEqual("Account has been locked please try again later", error.Message);
            }
        }

        [TestMethod]
        public void UnLockAccount()
        {
            IToken token = new TokenKey()
            {
                Token = "helloworld123key",
                LockOut = 3,
                LockExpire = DateTime.Now.AddMinutes(-30)
            };
            Sql.AlterDataQuery("update tokenkey set lockout = 3, lockexpire = @LockExpire Where token = @Token", token);
            var result = Authentication.Authentiacte(token, "user", "localhost");
            Assert.IsTrue(result.Token.Length > 10);
        }

        [TestMethod]
        public void ReauthenticateWithRefreshToken()
        {
            try
            {
                IToken token = new TokenKey()
                {
                    Token = "helloworld123key",
                };
                var auth = Authentication.Authentiacte(token, "user", "localhost");
                var table = Sql.SelectQuery("select * from getrefreshtoken(@RefreshToken)", auth);
                var result = Authentication.RefreshToken(auth.RefreshToken, "user", "localhost");
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
                IToken token = new TokenKey()
                {
                    Token = "helloworld123key",
                };
                var auth = Authentication.Authentiacte(token, "user", "localhost");
                var table = Sql.SelectQuery("select * from getrefreshtoken(@RefreshToken)", auth);
                var LockedRefToken = new RefreshToken() { Token = auth.RefreshToken };
                Sql.AlterDataQuery("update refreshtoken set used = true where token = @Token", LockedRefToken);
                var result = Authentication.RefreshToken(auth.RefreshToken, "user", "localhost");
                Assert.IsFalse(result.Token.Length > 3);
            }
            catch (Exception error)
            {
                Assert.AreEqual("Warning: The Refreshtoken has already been used", error.Message);
            }
        }
    }
}