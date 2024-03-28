using Api.Models;
using Api.Utils;

namespace Api.Interfaces
{
    public interface IMessageService
    {
        public Task<MessageModel> Create(MessageModel message);
        public Task<MessageModel> GetOne(int id);
        public Task<bool> DeleteMessage(int id);
        public Task DeleteAllMessageForRoom(RoomModel room);
        public Task<bool> UpdateMessage(int id, string text);
        public Task<Pagination<MessageModel>> FindMessagesForRoom(RoomModel room, PaginationOptions options);
    }
}
