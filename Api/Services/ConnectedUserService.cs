using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api.Services
{
    public class ConnectedUserService : IConnectedUserService
    {
        private readonly AppDbContext _context;

        public ConnectedUserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConnectedUserModel> CreateAsync(ConnectedUserModel connectedUser)
        {
            var entityEntry = await _context.ConnectedUsers.AddAsync(connectedUser);
            await _context.SaveChangesAsync();
            return entityEntry.Entity;
        }

        public async Task<IEnumerable<ConnectedUserModel>> FindByUserAsync(ConnectedUserModel user)
        {
            return await _context.ConnectedUsers
                .Where(cu => cu.UserId == user.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<ConnectedUserModel>> FindByRoomAsync(RoomModel room)
        {
            return await _context.ConnectedUsers
                .Where(cu => cu.User.Rooms.Any(r => r.Id == room.Id))
                .Include(cu => cu.User)
                .ToListAsync();
        }

        public async Task DeleteBySocketIdAsync(string socketId)
        {
            var connectedUsers = await _context.ConnectedUsers
                .Where(cu => cu.SocketId == socketId)
                .ToListAsync();

            _context.ConnectedUsers.RemoveRange(connectedUsers);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            _context.ConnectedUsers.RemoveRange(_context.ConnectedUsers);
            await _context.SaveChangesAsync();
        }
    }
}
