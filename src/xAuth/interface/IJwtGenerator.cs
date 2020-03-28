using System.Collections.Generic;
using System.Security.Claims;

namespace xAuth.Interface
{
    public interface IJwtGenerator
    {
        ITokenRespons CreateJwtToken(List<Claim> claim, string audiance, string issuer);
    }
}