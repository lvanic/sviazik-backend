using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class CallRoomService : ICallRoomService
    {
        private readonly AppDbContext _context;

        public CallRoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CallRoomModel> FindCallByRoom(RoomModel room)
        {
            return await _context.CallRooms
                .Include(call => call.Room)
                .FirstOrDefaultAsync(call => call.Room.Id == room.Id);
        }

        public async Task<CallRoomModel> FindByUserWithRoom(UserModel user)
        {
            return await _context.CallRooms
                .Include(call => call.Room)
                .Include(call => call.PeersUsers)
                    .ThenInclude(peer => peer.User)
                .FirstOrDefaultAsync(call => call.PeersUsers.Any(peer => peer.User.Id == user.Id));
        }

        public async Task<CallRoomModel> Create(CallRoomModel roomCall, UserModel creator, string peerId)
        {
            var callRoom = new CallRoomModel
            {
                Room = roomCall.Room,
                PeersUsers = new List<UserPeerModel>
            {
                new UserPeerModel
                {
                    User = creator,
                    PeerId = peerId
                }
            }
            };

            _context.CallRooms.Add(callRoom);
            await _context.SaveChangesAsync();

            return callRoom;
        }

        public async Task<CallRoomModel> FindByRoom(RoomModel room)
        {
            return await _context.CallRooms
                .Include(call => call.Room)
                .Include(call => call.PeersUsers)
                    .ThenInclude(peer => peer.User)
                .FirstOrDefaultAsync(call => call.Room.Id == room.Id);
        }

        public async Task<CallRoomModel> FindByUser(UserModel user)
        {
            return await _context.CallRooms
                .Include(call => call.Room)
                .Include(call => call.PeersUsers)
                    .ThenInclude(peer => peer.User)
                .FirstOrDefaultAsync(call => call.PeersUsers.Any(peer => peer.User.Id == user.Id));
        }

        public async Task DeleteOne(CallRoomModel roomCall)
        {
            _context.CallRooms.Remove(roomCall);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAll()
        {
            _context.CallRooms.RemoveRange(_context.CallRooms);
            await _context.SaveChangesAsync();
        }
    }
}
