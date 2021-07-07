using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{

    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context
                .Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context
                .Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking();

            return await PagedList<MemberDto>
                .CreateAsync(query, userParams.PageNumber, userParams.PageSize)
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
