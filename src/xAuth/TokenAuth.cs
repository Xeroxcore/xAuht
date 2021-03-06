using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class TokenAuth : Auth, IAuth
    {
        public TokenAuth(ISqlHelper sqlHandler, IJwtGenerator jwtGenerator)
        : base(sqlHandler, jwtGenerator) { }

        protected void FailedAuthentication(ILockout lockout)
            => Sql.AlterDataQuery<ILockout>("call faildtokenauth(@Id)", lockout);

        private void Unlock(ILockout lockout)
            => Sql.AlterDataQuery("call unlocktoken(@Id)", lockout);

        private void AddRefreshToken(string token, int id)
        {
            var reftoken = new { Token = token, Id = id };
            Sql.AlterDataQuery("call addfreshtokentoken(@Token,@Id)", reftoken);
        }

        public virtual ITokenRespons Authentiacte(object token, string audiance, string domain, AddClaimsMethod method)
        {

            try
            {
                var tokendb = GetAuthFromDB("select * from gettoken(@Token)", (TokenKey)token);
                var claims = FetchClaims(method, tokendb.Id);
                var tokenRespons = Jwt.CreateJwtToken(claims, audiance, domain);
                AddRefreshToken(tokenRespons.RefreshToken, tokendb.Id);
                if (!IsLocked(tokendb) && tokendb.LockOut > 0)
                    Unlock(tokendb);
                return tokenRespons;
            }
            catch
            {
                throw;
            }
        }

        public virtual ITokenRespons RefreshToken(string refreshtoken, string audiance, string domain, AddClaimsMethod method)
        {
            TokenKey tokendb = new TokenKey();
            try
            {
                var token = AuthRefreshToken(refreshtoken);
                tokendb.Id = token.TokenId;
                tokendb = GetAuthFromDB<TokenKey>("select * from gettokenbyid(@Id)", tokendb);
                return Authentiacte(tokendb, "token", "localhost", method); ;
            }
            catch
            {
                FailedAuthentication(tokendb);
                throw;
            }
        }
    }
}