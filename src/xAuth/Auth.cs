using System;
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

        private void Unlock(ILockout lockout)
        {
            lockout.LockOut = 0;
            lockout.LockExpire = DateTime.Now.AddMinutes(-15);
            Sql.AlterDataQuery("update useraccount set lockout = @lockout, lockexpire = @LockExpire where id = @Id", lockout);
        }

        private void IsLocked(ILockout lockout)
        {
            if (lockout.LockOut > 2)
                if (DateTime.Now < lockout.LockExpire.AddMinutes(15))
                    ThrowException($"Account has been locked please try again later");
                else
                    Unlock(lockout);
        }

        private void UserAccountIsValid(IUser dbuser, IUser user)
        {
            if (dbuser.UserName != user.UserName && dbuser.Password != user.Password)
                ThrowException($"User Authentication failed for {user.UserName}");
        }

        public virtual ITokenRespons AuthentiacteUser(IUser user, string audiance, string domain)
        {
            try
            {
                var account = Sql.SelectQuery<UserAccount>("select * from useraccount where username = @UserName", (UserAccount)user);
                if (account == null)
                    ThrowException("User not found");
                UserAccountIsValid(user, user);
                IsLocked(null);
                var tokenRespons = Jwt.CreateJwtToken(null, audiance, domain);
                return tokenRespons;
            }
            catch
            {
                throw;
            }
        }

        private void TokenKeyIsValid(IToken dbToken, IToken token)
        {
            if (dbToken.Token != token.Token)
                ThrowException($"User Authentication failed for {token.Token}");
        }

        public virtual ITokenRespons AuthenticateTokenKey(IToken token, string audiance, string domain)
        {
            try
            {
                var account = Sql.SelectQuery<TokenKey>("select * from tokenkey where token = @Token", (TokenKey)token);
                TokenKeyIsValid(token, token);
                IsLocked(null);
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
