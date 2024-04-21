using Api.Hubs;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ChatService _chatService;
        private readonly RoomService _roomService;
        public ChatController(UserService userService, ChatService chatService, RoomService roomService)
        {
            _userService = userService;
            _chatService = chatService;
            _roomService = roomService;
        }

        [HttpGet("connect")]
        public async Task<IActionResult> ConnectFromQuery([FromQuery] string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                int roomId = _chatService.DecryptToken(token);
                RoomModel room = await _roomService.GetRoomById(roomId);

                if (room != null)
                {
                    string frontendUrl = Request.HttpContext.Connection.RemoteIpAddress.ToString();//check how its working

                    var email = HttpContext.User.FindFirstValue(ClaimTypes.Email);
                    UserModel user = await _userService.FindByEmailAsync(email);

                    await _roomService.EnterRoom(room, user);
                    return Redirect($"{frontendUrl}/web");
                }
            }

            // Handle invalid token, frontend URL, or user not found scenario
            return BadRequest("Invalid token or user not found");
        }
    }
}
