using Api.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services
{
    public class ChatService : IChatService
    {
        private readonly IConfiguration _configuration;
        private readonly string _encryptionKey;

        public ChatService(IConfiguration configuration)
        {
            _configuration = configuration;
            _encryptionKey = _configuration["Jwt:Secret"] ?? throw new Exception("Key for tokens not set");
        }

        public string EncryptToken(int id)
        {
            using Aes aesAlg = Aes.Create();

            aesAlg.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            byte[] idBytes = Encoding.UTF8.GetBytes(id.ToString());

            byte[] encryptedBytes = encryptor.TransformFinalBlock(idBytes, 0, idBytes.Length);

            return Convert.ToBase64String(encryptedBytes);

        }

        public int DecryptToken(string token)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(token);

            using Aes aesAlg = Aes.Create();

            aesAlg.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            byte[] idBytes = decryptor.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

            return int.Parse(Encoding.UTF8.GetString(idBytes));

        }
    }
}
