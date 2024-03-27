using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Interfaces
{
    public interface RoomModelService
    {
        public Task<Pagination<RoomModel>> GetRoomsForUser(int userId, int page, int limit);
        public Task<RoomModel> UpdateRoom(RoomModel room);
        public Task<List<RoomModel>> GetRoomsByUserWhereIsCallTrue(UserModel user);
        public Task<RoomModel> CreateRoom(RoomModel room, UserModel creator);
        public Task DeleteRoom(RoomModel room);
        public Task<RoomModel> EnterRoom(RoomModel room, UserModel user);
        public Task RemoveUser(int roomId, UserModel user);
        public Task<List<RoomModel>> GetRoomsByName(string name);
        public Task<RoomModel> GetRoomByMessage(int messageId);
        
    }
}
