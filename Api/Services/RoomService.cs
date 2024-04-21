using Api.Data;
using Api.Interfaces;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;
using PagedList;

namespace Api.Services
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<RoomModel>> GetRoomsForUser(int userId, int page, int limit)
        {
            var user = await _context.Users.FindAsync(userId);
            var rooms = _context.Rooms
             .Where(x => x.Users.Contains(user))
             .Include(x => x.Messages)
             .OrderByDescending(x => x.Messages.OrderByDescending(m => m.UpdatedAt).FirstOrDefault().UpdatedAt)
             .OrderByDescending(y => y.UpdatedAt)
             .ToPagedList(page, limit);

            var pagination = new Pagination<RoomModel>
            {
                Page = page,
                Limit = limit,
                TotalItems = _context.Rooms.Count(),
                Items = rooms
            };

            return pagination;
        }
        public async Task<RoomModel> UpdateRoom(RoomModel room)
        {
            var RoomModel = await _context.Rooms.FindAsync(room.Id);
            if (RoomModel == null)
            {
                return null;
            }

            RoomModel.Name = room.Name;
            RoomModel.Description = room.Description;

            await _context.SaveChangesAsync();
            return RoomModel;
        }

        public async Task<List<RoomModel>> GetRoomsByUserWhereIsCallTrue(UserModel user)
        {
            var rooms = await _context.Rooms
                .Include(r => r.Users)
                .Where(r => r.Users.Any(u => u.Id == user.Id) && r.CallRoom.PeersUsers.Count > 0)
                .ToListAsync();

            return rooms;
        }

        public async Task<RoomModel> CreateRoom(RoomModel room, UserModel creator)
        {
            var creatorDb = await _context.Users.FindAsync(creator.Id);
            var RoomModel = new RoomModel
            {
                Name = room.Name,
                Description = room.Description,
                Admins = new List<UserModel> { creatorDb },
                Users = new List<UserModel> { creatorDb }
            };

            await _context.Rooms.AddAsync(RoomModel);
            await _context.SaveChangesAsync();

            return RoomModel;
        }

        public async Task DeleteRoom(RoomModel room)
        {
            var RoomModel = await _context.Rooms.FindAsync(room.Id);
            if (RoomModel != null)
            {
                _context.Rooms.Remove(RoomModel);
                _context.SaveChanges();
            }
            
        }

        public async Task<RoomModel> EnterRoom(RoomModel room, UserModel user)
        {
            var RoomModel = await _context.Rooms.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == room.Id);
            if (RoomModel == null || RoomModel.Users.Any(u => u.Id == user.Id))
            {
                return null;
            }

            RoomModel.Users.Add(user);
            await _context.SaveChangesAsync();

            return RoomModel;
        }

        public async Task RemoveUser(int roomId, UserModel user)
        {
            var RoomModel = await _context.Rooms.Include(x => x.Users).Where(x => x.Id == roomId).FirstOrDefaultAsync();
            if (RoomModel == null)
            {
                return;
            }

            RoomModel.Users.RemoveAll(u => u.Id == user.Id);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RoomModel>> GetRoomsByName(string name)
        {
            var rooms = await _context.Rooms
                .Where(r => EF.Functions.Like(r.Name, $"%{name}%"))
                .ToListAsync();

            return rooms;
        }

        public async Task<RoomModel> GetRoomByMessage(int messageId)
        {
            var room = await _context.Rooms
                .Include(r => r.Messages)
                .FirstOrDefaultAsync(r => r.Messages.Any(m => m.Id == messageId));

            return room;
        }

        public async Task<RoomModel> GetRoomById(int id)
        {
            return await _context.Rooms
                .Include(x => x.Users)
                .Include(x => x.Messages)
                .Include(x => x.Admins)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
