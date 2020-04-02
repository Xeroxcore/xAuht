using System.Collections.Generic;
using System.Security.Claims;
using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class UserAuth : Auth, IAuth
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
        private void ValidateAccount(UserAccount useracc, UserAccount userdb)
        {
            if (userdb.UserName != useracc.UserName || userdb.Password != useracc.Password)
            {
                FailedAuthentication(userdb);
                ThrowException($"Authentication failed for {useracc.UserName}");
            }

            if (!IsLocked(userdb) && userdb.LockOut > 0)
                Unlock(userdb);
        }

        public virtual ITokenRespons Authentiacte(object user, string audiance, string domain, AddClaimsMethod method)
        {
            try
            {
                var useracc = (UserAccount)user;
                var userdb = GetAuthFromDB("select * from getuser(@UserName)", useracc);
                ValidateAccount(useracc, userdb);
                var claims = FetchClaims(method, userdb.Id);
                var tokenRespons = Jwt.CreateJwtToken(claims, audiance, domain);
                AddRefreshToken(tokenRespons.RefreshToken, userdb.Id);
                return tokenRespons;
            }
            catch
            {
                throw;
            }
        }

        public ITokenRespons RefreshToken(string refreshtoken, string audiance, string domain, AddClaimsMethod method)
        {
            UserAccount user = new UserAccount();
            try
            {
                var token = AuthRefreshToken(refreshtoken);
                user.Id = token.UserId;
                user = GetAuthFromDB<UserAccount>("select * from getuserbyid(@Id)", user);
                return Authentiacte(user, "token", "localhost", method); ;
            }
            catch
            {
                throw;
            }
        }
    }
}