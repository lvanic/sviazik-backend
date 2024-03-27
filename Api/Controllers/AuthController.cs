using Api.Data;
using Api.Dto;
using Api.Interfaces;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;
        private readonly IAuthService _authorizationService;
        private readonly ICryptoService _cryptoService;

        public AuthController(ILogger<AuthController> logger, AppDbContext context, IAuthService authorizationService, ICryptoService cryptoService)
        {
            _logger = logger;
            _context = context;
            _authorizationService = authorizationService;
            _cryptoService = cryptoService;
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var token = await _authorizationService.GenerateJwt(user);
            var response = new LoginResponse
            {
                Email = user.Email,
                Username = user.Username,
                AccessToken = token,
                TokenType = "JWT",
                ExpiresIn = 10000,
                PublicKey = "publicKey"
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginUserDto loginUserDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginUserDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            if (!_cryptoService.VerifyPassword(loginUserDto.Password, user.Password))
                return Unauthorized("Invalid email or password.");

            var token = await _authorizationService.GenerateJwt(user);
            var response = new LoginResponse
            {
                Email = user.Email,
                Username = user.Username,
                AccessToken = token,
                TokenType = "JWT",
                ExpiresIn = 10000,
                PublicKey = "publicKey"
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostRegister(CreateUserDto createUserDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
            if (existingUser != null)
                return Conflict("Email is already in use.");

            var newUser = new UserModel
            {
                Email = createUserDto.Email,
                Username = createUserDto.Username,
                Password = _cryptoService.HashPassword(createUserDto.Password)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Created();
        }
    }
}
