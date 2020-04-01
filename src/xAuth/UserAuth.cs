using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class UserAuth : Auth
    {

        public UserAuth(ISqlHelper sqlHandler, JwtGenerator jwtGenerator) : base(sqlHandler, jwtGenerator)
        { }

        private void FailedAuthentication(ILockout lockout)
            => Sql.AlterDataQuery<ILockout>("call failduserauth(@Id)", lockout);

        protected void AddRefreshToken(string token, int id)
        {
            var reftoken = new { Token = token, Id = id };
            Sql.AlterDataQuery("call addfreshtokenuser(@Token, @Id)", reftoken);
        }

        private void Unlock(ILockout lockout)
            => Sql.AlterDataQuery("call unlockaccount(@Id)", lockout);

        public virtual ITokenRespons AuthentiacteUser(IUser user, string audiance, string domain)
        {
            try
            {
                var userdb = GetAuthFromDB("select * from getuser(@UserName)", (UserAccount)user);
                IsLocked(userdb, "user");
                if (userdb.UserName != user.UserName || userdb.Password != user.Password)
                {
                    FailedAuthentication(userdb);
                    ThrowException($"Authentication failed for {user.UserName}");
                }
                var tokenRespons = Jwt.CreateJwtToken(null, audiance, domain);
                AddRefreshToken(tokenRespons.RefreshToken, userdb.Id);
                if (userdb.LockOut > 0)
                    Unlock(userdb);
                return tokenRespons;
            }
            catch
            {
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