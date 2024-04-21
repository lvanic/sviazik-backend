using Api.Models;

namespace Api.Interfaces
{
    public interface IJoinedRoomService
    {
        public Task<IEnumerable<RoomModel>> FindRoomsByUserAsync(UserModel user);
        public Task<IEnumerable<JoinedRoomModel>> FindByUserAsync(UserModel user);
        public Task<JoinedRoomModel> CreateAsync(JoinedRoomModel joinedRoom);
        public Task<IEnumerable<JoinedRoomModel>> FindByRoomAsync(RoomModel room);
        public Task<int> CountByRoomAsync(RoomModel room);
        public Task<IEnumerable<JoinedRoomModel>> FindBySocketIdAsync(string socketId);
        public Task DeleteBySocketIdAsync(string socketId);
        public Task DeleteAllFromRoomAsync(RoomModel room);
        public Task DeleteAllAsync();
        public Task<IEnumerable<JoinedRoomModel>> GetAll();
        public Task DeleteByUserIdAsync(int userId);
    }
}
