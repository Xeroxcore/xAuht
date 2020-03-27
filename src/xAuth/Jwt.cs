using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using xAuth.Interface;

namespace xAuth
{
    public class Jwt : IJwtGenerator
    {
        private SymmetricSecurityKey EncryptionKey { get; }

        private string Algorithm { get; }
        public Jwt(string encryptionKey, string algorithm)
        {

            ConstructorParametersAreValid(encryptionKey, algorithm);
            EncryptionKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(encryptionKey));
        }

        private void ConstructorParametersAreValid(string encryptionKey, string algorithm)
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new Exception(
                    "You have provided an encryption key that is null or empty");

            if (string.IsNullOrWhiteSpace(algorithm))
                throw new Exception(
                    "Your have provided an algorithm is null or empty");
        }

        private string CreateRefreshToken()
        {
            var random = new Byte[32];
            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        private SigningCredentials CreateSigningCredentials()
            => new SigningCredentials(EncryptionKey, Algorithm);

        private object CreateToken(JwtSecurityToken token)
        {
            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                tokenType = "Bearer",
                expiration = $"UTC{token.ValidTo}",
                refreshToken = CreateRefreshToken()
            };
        }

        public virtual object CreateJwtToken()
        {
            var token = new JwtSecurityToken()
            {

            };
            throw new System.NotImplementedException();
        }
    }
}