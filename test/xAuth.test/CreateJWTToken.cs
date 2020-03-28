using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace xAuth.test
{
    [TestClass]
    public class CreateJWTToken
    {
        [TestMethod]
        public void CreateToken()
        {
            var jwt = new JwtGenerator("asdas1d31q51131#", "HS256");
            var token = jwt.CreateJwtToken(null, "user", "testdomain.com");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token.Token));
        }
    }
}
