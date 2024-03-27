using Api.Models;

namespace Api.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateJwt(UserModel user);
    }
}
