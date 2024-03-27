using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api.Services
{
    public class JoinedRoomService : IJoinedRoomService
    {
        private readonly AppDbContext _context;

        public JoinedRoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomModel>> FindRoomsByUserAsync(UserModel user)
        {
            var joinedRooms = await _context.JoinedRooms
                .Include(jr => jr.Room)
                .Where(jr => jr.User.Id == user.Id)
                .ToListAsync();

            return joinedRooms.Select(jr => jr.Room);
        }

        public async Task<IEnumerable<JoinedRoomModel>> FindByUserAsync(UserModel user)
        {
            return await _context.JoinedRooms
                .Include(jr => jr.Room)
                .Include(jr => jr.User)
                .Where(jr => jr.User.Id == user.Id)
                .ToListAsync();
        }

        public async Task<JoinedRoomModel> CreateAsync(JoinedRoomModel joinedRoom)
        {
            var entityEntry = await _context.JoinedRooms.AddAsync(joinedRoom);
            await _context.SaveChangesAsync();
            return entityEntry.Entity;
        }

        public async Task<IEnumerable<JoinedRoomModel>> FindByRoomAsync(RoomModel room)
        {
            return await _context.JoinedRooms
                .Include(jr => jr.User)
                .Where(jr => jr.Room.Id == room.Id)
                .ToListAsync();
        }

        public async Task<int> CountByRoomAsync(RoomModel room)
        {
            return await _context.JoinedRooms
                .CountAsync(jr => jr.Room.Id == room.Id);
        }

        public async Task<IEnumerable<JoinedRoomModel>> FindBySocketIdAsync(string socketId)
        {
            var roomHandler = await _context.JoinedRooms
                .Include(jr => jr.Room)
                .FirstOrDefaultAsync(jr => jr.SocketId == socketId);

            return await _context.JoinedRooms
                .Include(jr => jr.User)
                .Where(jr => jr.Room.Id == roomHandler.Room.Id)
                .ToListAsync();
        }

        public async Task DeleteBySocketIdAsync(string socketId)
        {
            var joinedRooms = await _context.JoinedRooms
                .Where(jr => jr.SocketId == socketId)
                .ToListAsync();

            _context.JoinedRooms.RemoveRange(joinedRooms);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllFromRoomAsync(RoomModel room)
        {
            var joinedRooms = await _context.JoinedRooms
                .Where(jr => jr.Room.Id == room.Id)
                .ToListAsync();

            _context.JoinedRooms.RemoveRange(joinedRooms);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            _context.JoinedRooms.RemoveRange(_context.JoinedRooms);
            await _context.SaveChangesAsync();
        }
    }
}
