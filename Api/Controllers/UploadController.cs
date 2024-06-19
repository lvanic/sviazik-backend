using Api.Hubs;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[DisableFormValueModelBinding]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IJoinedRoomService _joinedRoomService;
        private readonly IHubContext<ChatHub> _hubcontext;
        private readonly IConnectedUserService _connectedUserService;

        public UploadController(IRoomService roomService, IUserService userService, IMessageService messageService, IHubContext<ChatHub> hub, IJoinedRoomService joinedRoomService, IConnectedUserService connectedUserService)
        {
            _roomService = roomService;
            _userService = userService;
            _messageService = messageService;
            _hubcontext = hub;
            _joinedRoomService = joinedRoomService;
            _connectedUserService = connectedUserService;
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
        //upload image to room
        [HttpPost("room-image")]
        public async Task<IActionResult> UploadRoomImage(IFormFile file, RoomModel room)
        {
            var user = await _userService.FindByEmailAsync(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            var roomDb = await _roomService.GetRoomById(room.Id);
            using var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri("http://localhost:3003");

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
                roomDb.Image = responseContent;
                await _roomService.UpdateRoom(roomDb);
                return Ok(responseContent); // Верните responseContent в качестве ответа
            }
            else
            {
                return BadRequest(response);
            }
        }
        [HttpPost("message-image")]
        public async Task<IActionResult> UploadAttachment([FromForm] MessageFromForm form)
        {
            var message = form.Message;
            var file = form.File;
            var roomId = form.RoomId;

            var user = await _userService.FindByEmailAsync(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            var room = await _roomService.GetRoomById(roomId);
            using var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri("http://localhost:3003");

            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "image", file.FileName); // Замените "file" на "image", чтобы соответствовать вашему Node.js коду

            var response = await httpClient.PostAsync("/upload", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var messageHandler = new MessageModel()
                {
                    Text = message,
                    Room = room,
                    AttachmentType = AttachmentType.Image,
                    Attacment = responseContent,
                    MessageType = MessageType.User,
                    User = user,
                };

                var createdMessage = await _messageService.Create(messageHandler);

                var joinedUsers = await _connectedUserService.FindByRoomAsync(room);

                foreach (var joinedUser in joinedUsers)
                {
                    await _hubcontext.Clients.User(joinedUser.UserId.ToString()).SendAsync("messageAdded", createdMessage);
                }

                return Ok(responseContent); // Верните responseContent в качестве ответа
            }
            else
            {
                return BadRequest();
            }
        }
    }

    public class MessageFromForm
    {
        public IFormFile File { get; set; }
        public string Message { get; set; }
        public int RoomId { get; set; }
    }
}
