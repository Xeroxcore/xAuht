using System;
using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class AccountManager : IAccountManager
    {
        protected ISqlHelper Sql { get; }

        public AccountManager(ISqlHelper sql)
        {
            if (sql is null)
                throw new Exception("");
            Sql = sql;
        }

        public void addRoleToTokenKey(int tokenId, int roleId)
        {
            throw new System.NotImplementedException();
        }

        public void addRoleToUser(int userId, int roleId)
        {
            throw new System.NotImplementedException();
        }

        public void AddTokenKey(IToken newToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newToken.Token))
                    throw new Exception("The new token can't be null or empty or with space");
                Sql.AlterDataQuery("call addtoken(@Token)", newToken);
            }
            catch
            {
                throw;
            }
        }

        public void AddUser(string userName, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                    throw new Exception("The new useraccount can't be null or empty or with space");

                var newUser = new UserAccount() { UserName = userName, Password = password };
                Sql.AlterDataQuery("call adduser(@UserName, @Password)", newUser);
            }
            catch
            {
                throw;
            }
        }

        public void RemoveRolefromTokenKey(int tokenId, int roleId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRolefromUser(int userId, int roleId)
        {
            throw new System.NotImplementedException();
        }
    }
}