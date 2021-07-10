using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
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

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            _logger.LogInformation($"****************************************");
            _logger.LogInformation($"********************predicate:{likesParams.Predicate}, userId:{likesParams.UserId}********************");
            _logger.LogInformation($"****************************************");

            var users = _context.Users.OrderBy(x => x.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();
            if(likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.LikedUser);
            }
            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.ProjectTo<LikeDto>(_mapper.ConfigurationProvider);

            return await PagedList<LikeDto>
                .CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize)
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
