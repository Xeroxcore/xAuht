﻿using System;
using Components;
using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class Auth : IAuth
    {
        protected ISqlHelper Sql { get; }
        protected IJwtGenerator Jwt { get; }
        public int AuthAthempts = 3;
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
            if (lockout.LockOut == AuthAthempts)
                if (lockout.LockExpire.AddMinutes(15) > DateTime.Now)
                    ThrowException($"Account has been locked please try again later");
                else
                    Unlock(lockout);
        }

        private void FailedAuthentication(ILockout lockout)
        {
            if (lockout.LockOut < AuthAthempts)
                lockout.LockOut += 1;

            if (lockout.LockOut < AuthAthempts)
                Sql.AlterDataQuery<ILockout>("update useraccount set lockout = @LockOut where id = @Id", lockout);
            else
                Sql.AlterDataQuery<ILockout>("update useraccount set lockout = @LockOut, lockexpire = now() where id = @Id", lockout);


        }

        public virtual ITokenRespons AuthentiacteUser(IUser user, string audiance, string domain)
        {
            try
            {
                var userdb = GetAuthFromDB("select * from getuser(@UserName)", (UserAccount)user);
                IsLocked(userdb);
                if (userdb.UserName != user.UserName || userdb.Password != user.Password)
                {
                    FailedAuthentication(userdb);
                    ThrowException($"Authentication failed for {user.UserName}");
                }
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
                IsLocked(tokendb);
                if (tokendb.Token != token.Token)
                {
                    FailedAuthentication(tokendb);
                    ThrowException($"Authentication failed for {token.Token}");
                }

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
