using System.Security.Cryptography;
using System.Text;

namespace Api.Interfaces
{
    public interface IChatService
    {
        public string EncryptToken(int id);

        public int DecryptToken(string token);
    }
}
