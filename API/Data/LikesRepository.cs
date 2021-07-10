using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<LikesRepository> _logger;

        public LikesRepository(DataContext context, IMapper mapper, ILogger<LikesRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserdId)
        {
            return await _context
                .Likes
                .FindAsync(sourceUserId, likedUserdId)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            _logger.LogInformation($"****************************************");
            _logger.LogInformation($"********************predicate:{predicate}, userId:{userId}********************");
            _logger.LogInformation($"****************************************");

            var users = _context.Users.OrderBy(x => x.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();
            if(predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == userId);
                users = likes.Select(like => like.LikedUser);
            }
            if (predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == userId);
                users = likes.Select(like => like.SourceUser);
            }

            return await users
                .ProjectTo<LikeDto>(_mapper.ConfigurationProvider)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context
                .Users
                .Include(x => x.LikedByUsers)
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId)
                .ConfigureAwait(false);
        }
    }
}
