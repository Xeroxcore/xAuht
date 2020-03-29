using System;
using System.Linq;
using Components;
using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class Auth : IAuth
    {
        protected ISqlHelper Sql { get; }
        protected IJwtGenerator Jwt { get; }
        public Auth(ISqlHelper sqlHandler, JwtGenerator jwtGenerator)
        {
            if (sqlHandler == null)
                ThrowException("sqlHandler is null");

            if (jwtGenerator == null)
                ThrowException("jwtGenerator is null");

            Sql = sqlHandler;
            Jwt = jwtGenerator;
        }

        protected void ThrowException(string text)
            => throw new Exception(text);

        protected T GetAuthFromDB<T>(string querry, T data)
        {
            var table = Sql.SelectQuery<T>(querry, (T)data);
            if (table.Rows.Count < 1)
                ThrowException("Value not found in DB");
            return ObjectConverter.ConvertDataTableRowToObject<T>(table, 0);
        }

        private void Unlock(ILockout lockout)
        {
            lockout.LockOut = 0;
            lockout.LockExpire = DateTime.Now.AddMinutes(-15);
            Sql.AlterDataQuery("update useraccount set lockout = @LockOut, lockexpire = @LockExpire where id = @Id", lockout);
        }

        private void IsLocked(ILockout lockout)
        {
            if (lockout.LockOut > 2)
                if (lockout.LockExpire.AddMinutes(15) > DateTime.Now)
                    ThrowException($"Account has been locked please try again later");
                else
                    Unlock(lockout);
        }

        private void FailedAuthentication(ILockout lockout)
        {
            if (lockout.LockOut < 3)
                Sql.AlterDataQuery<Lockout>("update useraccount set lockout = @LockOut where id = @Id", null);
            else
                Sql.AlterDataQuery<Lockout>("update useraccount set lockexpire = now() where id = @Id", null);
        }

        private void AccountIsValid(string passedValue, string dbValue)
        {
            if (passedValue != dbValue)
                ThrowException($"Authentication failed for {passedValue}");
        }

        public virtual ITokenRespons AuthentiacteUser(IUser user, string audiance, string domain)
        {
            try
            {
                var userdb = GetAuthFromDB("select * from getuser(@UserName)", (UserAccount)user);
                IsLocked((Lockout)userdb);
                AccountIsValid(userdb.UserName, user.UserName);
                var tokenRespons = Jwt.CreateJwtToken(null, audiance, domain);
                return tokenRespons;
            }
            catch
            {
                throw;
            }
        }

        public virtual ITokenRespons AuthenticateTokenKey(IToken token, string audiance, string domain)
        {
            try
            {
                var tokendb = GetAuthFromDB("select * from gettoken(@Token)", (TokenKey)token);
                IsLocked((Lockout)tokendb);
                AccountIsValid(tokendb.Token, token.Token);
                var tokenRespons = Jwt.CreateJwtToken(null, audiance, domain);
                return tokenRespons;
            }
            catch
            {
                throw;
            }
        }
    }
}
