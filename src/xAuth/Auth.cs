using System;
using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class Auth : IAuth
    {
        private ISqlHelper Sql { get; }
        public Auth(ISqlHelper sqlHandler)
        {
            if (sqlHandler == null)
                throw new Exception("sqlHandler can't be null");
            Sql = sqlHandler;
        }

        public ITokenRespons AuthentiacteUser(IUser user)
        {
            try
            {
                return null;
            }
            catch
            {
                throw;
            }
            throw new System.NotImplementedException();
        }

        public ITokenRespons AuthenticateTokenKey(IToken token)
        {
            try
            {
                return null;
            }
            catch
            {
                throw;
            }
            throw new System.NotImplementedException();
        }
    }
}
