using Api.Interfaces;
using Api.Security;

namespace Api.Services
{
    public class CryptoService: ICryptoService
    {
        public string HashPassword(string password) => PasswordHasher.HashPassword(password);

        public bool VerifyPassword(string password, string hashedPassword) => PasswordHasher.VerifyPassword(password, hashedPassword);

    }
}
