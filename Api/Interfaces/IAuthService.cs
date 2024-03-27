using Api.Models;
using System.Security.Claims;

namespace Api.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateJwt(UserModel user);
        public ClaimsPrincipal GetClaimsPrincipal(UserModel user);
    }
}
