using System;
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
            if (table != null && table.Rows.Count < 1)
                ThrowException("Value not found in DB");
            return ObjectConverter.ConvertDataTableRowToObject<T>(table, 0);
        }

        private void Unlock(ILockout lockout, string type)
        {
            if ("user" == type)
                Sql.AlterDataQuery("call unlockaccount(@Id)", lockout);
            else
                Sql.AlterDataQuery("call unlocktoken(@Id)", lockout);
        }

        protected void IsLocked(ILockout lockout, string type)
        {
            if (lockout.LockOut >= 3)
                if (lockout.LockExpire.AddMinutes(15) > DateTime.Now)
                    ThrowException($"Account has been locked please try again later");
                else
                    Unlock(lockout, type);
        }

        protected void FailedAuthentication(ILockout lockout, string type)
        {
            if ("user" == type.ToLower())
                Sql.AlterDataQuery<ILockout>("call failduserauth(@Id)", lockout);
            else
                Sql.AlterDataQuery<ILockout>("call faildtokenauth(@Id)", lockout);
        }

        private void AddRefreshToken(string token, int id, string type)
        {
            var reftoken = new { Token = token, Id = id };
            if (type == "user")
                Sql.AlterDataQuery("call addfreshtokenuser(@Token,@Id)", reftoken);
            else
                Sql.AlterDataQuery("call addfreshtokentoken(@Token,@Id)", reftoken);
        }

        public virtual ITokenRespons AuthentiacteUser(IUser user, string audiance, string domain)
        {
            try
            {
                var userdb = GetAuthFromDB("select * from getuser(@UserName)", (UserAccount)user);
                IsLocked(userdb, "user");
                if (userdb.UserName != user.UserName || userdb.Password != user.Password)
                {
                    FailedAuthentication(userdb, "user");
                    ThrowException($"Authentication failed for {user.UserName}");
                }
                var tokenRespons = Jwt.CreateJwtToken(null, audiance, domain);
                AddRefreshToken(tokenRespons.RefreshToken, userdb.Id, "user");
                if (userdb.LockOut > 0)
                    Unlock(userdb, "user");
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
                IsLocked(tokendb, "token");
                var tokenRespons = Jwt.CreateJwtToken(null, audiance, domain);
                AddRefreshToken(tokenRespons.RefreshToken, tokendb.Id, "token");
                if (tokendb.LockOut > 0)
                    Unlock(tokendb, "token");
                return tokenRespons;
            }
            catch
            {
                throw;
            }
        }

        protected void RefreshTokenIsValid(IRefreshToken refreshtoken)
        {
            if (refreshtoken.Used)
                ThrowException("Warning: The Refreshtoken has already been used");

            if (refreshtoken.Expired.AddMinutes(20) < DateTime.Now)
                ThrowException("Warning: The Refreshtoken has expired" + refreshtoken.Expired);
        }

        protected IRefreshToken GetRefreshToken(string refreshtoken)
        {
            var tokenkey = new RefreshToken()
            {
                Token = refreshtoken
            };
            var table = Sql.SelectQuery("select * from getrefreshtoken(@Token)", tokenkey);
            return ObjectConverter.ConvertDataTableRowToObject<RefreshToken>(table, 0);
        }

        private IRefreshToken AuthRefreshToken(string refreshtoken)
        {
            var token = GetRefreshToken(refreshtoken);
            RefreshTokenIsValid(token);
            return token;
        }

        public virtual ITokenRespons RefreshTokenKey(string refreshtoken, string audiance, string domain)
        {
            TokenKey tokendb = new TokenKey();
            try
            {
                var token = AuthRefreshToken(refreshtoken);
                tokendb.Id = token.TokenId;
                tokendb = GetAuthFromDB<TokenKey>("select * from gettokenbyid(@Id)", tokendb);
                return AuthenticateTokenKey(tokendb, "token", "localhost"); ;
            }
            catch
            {
                FailedAuthentication(tokendb, "token");
                throw;
            }

        }

        public ITokenRespons RefreshUserAccount(string refreshtoken, string audiance, string domain)
        {
            UserAccount user = new UserAccount();
            try
            {
                var token = AuthRefreshToken(refreshtoken);
                user.Id = token.UserId;
                user = GetAuthFromDB<UserAccount>("select * from getuserbyid(@Id)", user);
                return AuthentiacteUser(user, "token", "localhost"); ;
            }
            catch
            {
                throw;
            }
        }
    }
}
