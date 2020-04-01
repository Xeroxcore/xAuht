using System;
using Components;
using xAuth.Interface;
using xSql.Interface;

namespace xAuth
{
    public class Auth
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

        protected bool IsLocked(ILockout lockout)
        {
            if (lockout.LockOut >= 3)
                if (lockout.LockExpire.AddMinutes(15) > DateTime.Now)
                    ThrowException($"Account has been locked please try again later");

            return false;
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

        protected void RefreshTokenIsValid(IRefreshToken refreshtoken)
        {
            if (refreshtoken.Used)
                ThrowException("Warning: The Refreshtoken has already been used");

            if (refreshtoken.Expired.AddMinutes(20) < DateTime.Now)
                ThrowException("Warning: The Refreshtoken has expired" + refreshtoken.Expired);
        }

        protected IRefreshToken AuthRefreshToken(string refreshtoken)
        {
            var token = GetRefreshToken(refreshtoken);
            RefreshTokenIsValid(token);
            return token;
        }
    }
}
