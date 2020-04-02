namespace xAuth.Interface
{
    public interface IAccountManager
    {
        void AddUser(string userName, string password);
        void AddTokenKey(IToken newToken);
        void addRoleToUser(int userId, int roleId);
        void addRoleToTokenKey(int tokenId, int roleId);
        void RemoveRolefromUser(int userId, int roleId);
        void RemoveRolefromTokenKey(int tokenId, int roleId);
    }
}