using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interface;
using AutoMapper;
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
        public Task<UserLike> GetUserLike(int sourceUserId, int likedUserdId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<AppUser> GetUserWithLikes(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
