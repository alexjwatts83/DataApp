using System.Collections.Generic;
using System.Threading.Tasks;
using API.Entities;
using API.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{

    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<AppUser> GerUserByIdAsync(int id)
        {
            return await _context
                .Users
                .FindAsync(id)
                .ConfigureAwait(false);
        }

        public async Task<AppUser> GerUserByUsernameAsync(string username)
        {
            return await _context
                .Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context
                .Users
                .Include(x => x.Photos)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> SallAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
