using xAuth.Interface;

namespace xAuth
{
    public class Role : IRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}