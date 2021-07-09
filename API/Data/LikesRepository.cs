using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interface;
using AutoMapper;
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

            // TODO: add to IMapper later on
            return await users
                .Include(x => x.Photos) // TODO: double check if this is actually needed
                .Select(user => new LikeDto() {
                    Username = user.UserName,
                    KnownAs = user.KnownAs,
                    Age = user.DateOfBirth.CalculateAge(),
                    City = user.City,
                    Id = user.Id,
                    PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context
                .Users
                .Include(x => x.LikedByUsers)
                .FirstOrDefaultAsync(x => x.Id == userId)
                .ConfigureAwait(false);
        }
    }
}
