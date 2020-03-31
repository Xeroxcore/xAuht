namespace xAuth.Interface
{
    public interface IAuth
    {
        ITokenRespons AuthentiacteUser(IUser user, string audiance, string domain);
        ITokenRespons AuthenticateTokenKey(IToken token, string audiance, string domain);
        ITokenRespons RefreshTokenKey(string refreshtoken, string audiance, string domain);
        ITokenRespons RefreshUserAccount(string refreshtoken, string audiance, string domain);
    }
}