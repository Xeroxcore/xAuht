namespace xAuth.Interface
{
    public interface IAuth
    {
        ITokenRespons Authentiacte(object account, string audiance, string domain, AddClaimsMethod method);
        ITokenRespons RefreshToken(string refreshtoken, string audiance, string domain, AddClaimsMethod method);
    }
}