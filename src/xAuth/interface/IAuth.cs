namespace xAuth.Interface
{
    public interface IAuth
    {
        ITokenRespons AuthentiacteUser(IUser user);

        ITokenRespons AuthenticateTokenKey(IToken token);

    }
}