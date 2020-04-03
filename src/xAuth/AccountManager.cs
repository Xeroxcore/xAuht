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
                throw new Exception("sql Parameter cant be null in constructor");
            Sql = sql;
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

        private void RoleDataIsValid(int dbId, int roleId)
        {
            if (dbId == 0 && roleId == 0)
                throw new Exception("You can't pass 0 as ID");
        }

        public void addRoleToTokenKey(int tokenId, int roleId)
        {
            try
            {
                RoleDataIsValid(tokenId, roleId);
                var role = new { TokenId = tokenId, RoleId = roleId };
                Sql.AlterDataQuery("call addtokenrole(@TokenId, @RoleId)", role);
            }
            catch
            {
                throw;
            }
        }

        public void addRoleToUser(int userId, int roleId)
        {

            try
            {
                RoleDataIsValid(userId, roleId);
                var role = new { UserId = userId, RoleId = roleId };
                Sql.AlterDataQuery("call adduserrole(@UserId, @RoleId)", role);
            }
            catch
            {
                throw;
            }
        }

        public void RemoveRolefromTokenKey(int tokenId, int roleId)
        {
            try
            {
                RoleDataIsValid(tokenId, roleId);
                var role = new { TokenId = tokenId, RoleId = roleId };
                Sql.AlterDataQuery("call removerolefromtoken(@TokenId, @RoleId)", role);
            }
            catch
            {
                throw;
            }
        }

        public void RemoveRolefromUser(int userId, int roleId)
        {
            try
            {
                RoleDataIsValid(userId, roleId);
                var role = new { UserId = userId, RoleId = roleId };
                Sql.AlterDataQuery("call removerolefromuser(@UserId, @RoleId)", role);
            }
            catch
            {
                throw;
            }
        }
    }
}