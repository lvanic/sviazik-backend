using Api.Data;
using Api.Interfaces;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;

        public MessageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MessageModel> Create(MessageModel message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<MessageModel> GetOne(int id)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAllMessageForRoom(RoomModel room)
        {
            var messages = await _context.Messages
                .Where(m => m.RoomId == room.Id)
                .ToListAsync();

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateMessage(int id, string text)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return false;
            }

            message.Text = text;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Pagination<MessageModel>> FindMessagesForRoom(RoomModel room, PaginationOptions options)
        {
            var query = _context.Messages
                .Where(m => m.RoomId == room.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.User);

            var totalItems = await query.CountAsync();
            var messages = await query.Skip((options.Page - 1) * options.Limit)
                                      .Take(options.Limit)
                                      .ToListAsync();

            return new Pagination<MessageModel>()
            {
                Items = messages,
                TotalItems = totalItems,
                Limit = options.Limit,
                Page = options.Page,
            };
        }
    }
}
