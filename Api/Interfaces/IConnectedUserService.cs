using Api.Models;

namespace Api.Interfaces
{
    public interface IConnectedUserService
    {
        public Task<ConnectedUserModel> CreateAsync(ConnectedUserModel connectedUser);
        public Task<IEnumerable<ConnectedUserModel>> FindByUserAsync(ConnectedUserModel user);
        public Task<IEnumerable<ConnectedUserModel>> FindByRoomAsync(RoomModel room);
        public Task DeleteBySocketIdAsync(string socketId);
        public Task DeleteAllAsync();
    }
}
