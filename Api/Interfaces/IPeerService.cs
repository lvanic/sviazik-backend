using Api.Models;

namespace Api.Interfaces
{
    public interface IPeerService
    {
        public Task<UserPeerModel> AddPeerUserAsync(UserPeerModel peerUser);
        public Task DeletePeerUserAsync(UserPeerModel peerUser);
        public Task<UserPeerModel> GetOneAsync(string peerId);
        public Task<UserPeerModel> GetOneByUserAsync(UserModel user);

    }
}
