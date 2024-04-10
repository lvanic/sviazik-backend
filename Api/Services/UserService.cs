using Api.Data;
using Api.Interfaces;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;
using PagedList;

namespace Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserModel>> FindAllAsync(PaginationOptions options)
        {
            var query = _context.Users.AsQueryable();
            var pagedUsers = query.ToPagedList(options.Page, options.Limit);
            return pagedUsers;
        }

        public async Task<UserModel> FindByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower().Contains(username.ToLower()));
        }

        public async Task<UserModel> FindByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserModel> GetOneAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new Exception("No such user");
        }

        public Task<IEnumerable<UserModel>> GetAll()
        {
            return (Task<IEnumerable<UserModel>>)_context.Users.AsEnumerable();
        }

        public async Task<UserModel> UpdateUser(UserModel user)
        {
            var entityEntry = _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return entityEntry.Entity;
        }
    }
}
