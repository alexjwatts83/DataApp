using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
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

        Task<IEnumerable<MemberDto>> GetMembersAsync();
        Task<MemberDto> GetMemberAsync(string username);
    }
}
