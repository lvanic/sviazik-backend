using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Interfaces
{
    public interface ICallRoomService
    {
        public Task<CallRoomModel> FindCallByRoom(RoomModel room);
        public Task<CallRoomModel> FindByUserWithRoom(UserModel user);
        public Task<CallRoomModel> Create(CallRoomModel roomCall, UserModel creator, string peerId);
        public Task<CallRoomModel> FindByRoom(RoomModel room);
        public Task<CallRoomModel> FindByUser(UserModel user);
        public Task DeleteOne(CallRoomModel roomCall);
        public Task DeleteAll();
       
    }
}
