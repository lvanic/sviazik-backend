using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public UploadController(IRoomService roomService, IUserService userService)
        {
            _roomService = roomService;
            _userService = userService;
        }

        //upload avatar to user
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var user = await _userService.FindByEmailAsync(HttpContext.User.FindFirstValue(ClaimTypes.Email));

            using var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri("http://localhost:3003"); // Адрес вашего Node.js сервера

            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "image", file.FileName); // Замените "file" на "image", чтобы соответствовать вашему Node.js коду
            //content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data"); // Установите правильный Content-Type

            var response = await httpClient.PostAsync("/upload", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                user.Image = responseContent;
                await _userService.UpdateUser(user);
                return Ok(responseContent); // Верните responseContent в качестве ответа
            }
            else
            {
                return BadRequest(response);
            }
        }

    }
}
