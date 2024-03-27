using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class PeerService : IPeerService
    {
        private readonly AppDbContext _context;

        public PeerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserPeerModel> AddPeerUserAsync(UserPeerModel peerUser)
        {
            _context.UserPeers.Add(peerUser);
            await _context.SaveChangesAsync();
            return peerUser;
        }

        public async Task DeletePeerUserAsync(UserPeerModel peerUser)
        {
            _context.UserPeers.Remove(peerUser);
            await _context.SaveChangesAsync();
        }

        public async Task<UserPeerModel> GetOneByUserAsync(UserModel user)
        {
            return await _context.UserPeers
                .FirstOrDefaultAsync(p => p.UserId == user.Id) ?? throw new Exception("No such peer");
        }

        public async Task<UserPeerModel> GetOneAsync(string peerId)
        {
            return await _context.UserPeers
                .FirstOrDefaultAsync(p => p.PeerId == peerId) ?? throw new Exception("No such peer");
        }
    }
}
