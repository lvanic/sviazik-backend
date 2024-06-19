using Api.Controllers;
using Api.Data;
using Api.Dto;
using Api.Interfaces;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace api.Tests
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:Secret"]).Returns("qwertyuiopasdfgzqwertyuiopasdfgz");
            _configurationMock.Setup(c => c["Jwt:ExpirationInMinutes"]).Returns("360");
            _authService = new AuthService(_configurationMock.Object);
        }

        [TestMethod]
        public async Task GenerateJwt_ShouldReturnValidToken()
        {
            var user = new UserModel { Id = 1, Email = "test@example.com" };

            var token = await _authService.GenerateJwt(user);

            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token));

            var handler = new JwtSecurityTokenHandler();
            var validatedToken = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configurationMock.Object["Jwt:Secret"])),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true
            }, out _);

            Assert.IsNotNull(validatedToken);
            Assert.AreEqual(user.Id.ToString(), validatedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            Assert.AreEqual(user.Email, validatedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("User", validatedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        }

        [TestMethod]
        public async Task VerifyJwt_ShouldReturnClaimsPrincipal()
        {
            // Arrange
            var user = new UserModel { Id = 1, Email = "test@example.com" };
            var token = await _authService.GenerateJwt(user);

            // Act
            var claimsPrincipal = await _authService.VerifyJwt(token);

            // Assert
            Assert.IsNotNull(claimsPrincipal);
            Assert.AreEqual(user.Id.ToString(), claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.AreEqual(user.Email, claimsPrincipal.FindFirstValue(ClaimTypes.Email));
            Assert.AreEqual("User", claimsPrincipal.FindFirstValue(ClaimTypes.Role));
        }

        [TestMethod]
        public void GetClaimsPrincipal_ShouldReturnClaimsPrincipal()
        {
            var user = new UserModel { Id = 1, Email = "test@example.com" };

            var claimsPrincipal = _authService.GetClaimsPrincipal(user);

            Assert.IsNotNull(claimsPrincipal);
            Assert.AreEqual(user.Id.ToString(), claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.AreEqual(user.Email, claimsPrincipal.FindFirstValue(ClaimTypes.Email));
            Assert.AreEqual("User", claimsPrincipal.FindFirstValue(ClaimTypes.Role));
        }
    }
}