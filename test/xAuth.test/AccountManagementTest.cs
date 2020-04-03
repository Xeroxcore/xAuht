using System;
using System.Security.Cryptography;
using Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xAuth.Interface;
using xSql;
using xSql.Interface;

namespace xAuth.test
{
    [TestClass]
    public class AccountManagmentTest
    {
        ISqlHelper Sql = new NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld");
        IAccountManager manger = new AccountManager(new NpgSql("Server=127.0.0.1;port=5432;Database=testdb;Uid=testuser;Pwd=helloworld"));
        [TestMethod]
        public void CreateUser()
        {
            var newUser = new UserAccount() { UserName = "Nasar3", Password = "helloworld123" };
            manger.AddUser("Nasar3", "helloworld123");
            var table = Sql.SelectQuery("select * from getuser(@UserName)", newUser);
            var result = ObjectConverter.ConvertDataTableRowToObject<UserAccount>(table, 0);
            Assert.AreEqual("Nasar3", result.UserName);
        }

        [TestMethod]
        public void AddUserWithSameUserName()
        {
            var newUser = new UserAccount() { UserName = "Nasar3", Password = "helloworld123" };
            try
            {
                manger.AddUser("Nasar3", "helloworld123");

                Assert.AreNotEqual("Nasar3", newUser.UserName);
            }
            catch (Exception error)
            {
                var table = Sql.SelectQuery("select * from getuser(@UserName)", newUser);
                var user = ObjectConverter.ConvertDataTableRowToObject<UserAccount>(table, 0);
                Sql.AlterDataQuery("delete from useraccount where id = @Id", user);
                Assert.AreEqual("23505: duplicate key value violates unique constraint \"username\"", error.Message);
            }
        }

        private string CreateRefreshToken()
        {
            var random = new Byte[32];
            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        [TestMethod]
        public void CreateToken()
        {
            var token = new TokenKey() { Token = CreateRefreshToken() };
            manger.AddTokenKey(token);
            var table = Sql.SelectQuery("select * from gettoken(@Token)", token);
            var result = ObjectConverter.ConvertDataTableRowToObject<TokenKey>(table, 0);
            Assert.AreEqual(token.Token, result.Token);
            Sql.AlterDataQuery("delete from tokenkey where token = @Token", token);
        }
    }
}