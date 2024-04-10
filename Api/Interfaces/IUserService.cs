using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Interfaces
{
    public interface IUserService
    {
        public Task<IEnumerable<UserModel>> FindAllAsync(PaginationOptions options);
        public Task<UserModel> FindByUsernameAsync(string username);
        public Task<UserModel> FindByEmailAsync(string email);
        public Task<UserModel> GetOneAsync(int id);
        public Task<IEnumerable<UserModel>> GetAll();
        public Task<UserModel> UpdateUser(UserModel user);
    }
}
