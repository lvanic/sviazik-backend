using Api.Models;
using System.Security.Claims;

namespace Api.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateJwt(UserModel user);
        public ClaimsPrincipal GetClaimsPrincipal(UserModel user);
        public Task<ClaimsPrincipal> VerifyJwt(string token);
    }
}
