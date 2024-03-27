using Api.Interfaces;
using Api.Models;
using Api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomModelService _roomService;

        public RoomsController(IRoomModelService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<ActionResult<Pagination<RoomModel>>> GetRooms([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var rooms = await _roomService.GetRoomsForUser(userId, page, limit);

            if (rooms == null)
            {
                return NotFound();
            }

            return Ok(rooms);
        }
    }
}
