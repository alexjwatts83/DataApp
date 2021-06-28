using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.Interface
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SallAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GerUserByIdAsync(int id);
        Task<AppUser> GerUserByUsernameAsync(string username);
    }
}
