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
        private readonly IAuth Authentication = new Auth(
            new NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld"),
            new JwtGenerator("asdas1d31q51131#", "HS256"));

        [TestMethod]
        public void AuthenticateTokenKey()
        {
            IToken user = new TokenKey()
            {
                Token = "helloworld123key"
            };
            var result = Authentication.AuthenticateTokenKey(user, "tokenkey-", "localhost");
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
                var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
                sql.AlterDataQuery("update tokenkey set lockout = @LockOut, lockexpire = now() Where token = @Token", token);
                var result = Authentication.AuthenticateTokenKey(token, "user", "localhost");
            }
            catch (Exception error)
            {
                Assert.AreEqual("Account has been locked please try again later", error.Message);
            }
        }

        [TestMethod]
        public void LockAccount()
        {
            IToken token = new TokenKey()
            {
                Token = "helloworld123key",
                LockOut = 3,
            };
            var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
            sql.AlterDataQuery("update tokenkey set lockout = 2, lockexpire = now() Where token = @Token", token);
            try
            {
                var reuslt = Authentication.AuthenticateTokenKey(token, "user", "localhost");
            }
            catch
            {
                var table = sql.SelectQuery("select * from gettoken(@Token)", token);
                var dbUser = ObjectConverter.ConvertDataTableRowToObject<TokenKey>(table, 0);
                Assert.AreEqual(3, dbUser.LockOut);
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
            var sql = new xSql.NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
            sql.AlterDataQuery("update tokenkey set lockout = 3, lockexpire = @LockExpire Where token = @Token", token);
            var result = Authentication.AuthenticateTokenKey(token, "user", "localhost");
            Assert.IsTrue(result.Token.Length > 10);
        }

        [TestMethod]
        public void ReauthenticateWithRefreshToken()
        {
            IToken token = new TokenKey()
            {
                Token = "helloworld123key",
            };
            var auth = Authentication.AuthenticateTokenKey(token, "user", "localhost");
            var result = Authentication.RefreshTokenKey(auth.refreshToken, "user", "localhost");
            Assert.IsTrue(result.Token.Length > 3);
        }
    }
}