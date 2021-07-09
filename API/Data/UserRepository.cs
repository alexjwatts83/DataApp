using System;
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
using Microsoft.Extensions.Logging;

namespace API.Data
{

    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(DataContext context, IMapper mapper, ILogger<UserRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context
                .Users
                .FindAsync(id)
                .ConfigureAwait(false);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
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
                .AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDbo = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDbo = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDbo && u.DateOfBirth <= maxDbo);

            // TODO: look up switch expressions
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                "age" => query.OrderByDescending(x => x.DateOfBirth),
                _ => query.OrderByDescending(x => x.LastActive)
            };

            _logger.LogInformation($"********************CurrentUsername:{userParams.CurrentUsername}********************");
            _logger.LogInformation($"********************Gender:{userParams.Gender}********************");
            _logger.LogInformation($"********************MaxAge:{userParams.MaxAge}********************");
            _logger.LogInformation($"********************MinAge:{userParams.MinAge}********************");
            _logger.LogInformation($"********************OrderBy:{userParams.OrderBy}********************");

            return await PagedList<MemberDto>
                .CreateAsync(query
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .AsNoTracking(), userParams.PageNumber, userParams.PageSize)
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

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
