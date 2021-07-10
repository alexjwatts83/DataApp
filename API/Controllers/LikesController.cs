using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize]
    [EnableCors("AllowOrigin")]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        private readonly ILogger<LikesController> _logger;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository, ILogger<LikesController> logger)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
            _logger = logger;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();

            var likedUser = await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId).ConfigureAwait(false);

            _logger.LogInformation($"********************likedUser.UserName:{likedUser.UserName}********************");
            _logger.LogInformation($"********************sourceUser.UserName:{sourceUser.UserName}********************");

            if (likedUser == null)
            {
                return NotFound($"Liked user '{username}' not found");
            }

            if(sourceUser.UserName == username)
            {
                return BadRequest($"Cannot like yourself {sourceUser.KnownAs}, do better");
            }

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id).ConfigureAwait(false);

            if(userLike != null)
            {
                return BadRequest($"{sourceUser.KnownAs}, you already liked {likedUser.KnownAs}");
            }

            _logger.LogInformation($"********************SourceUserId:{sourceUserId},LikedUserId:{likedUser.Id}********************");

            userLike = new UserLike()
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            _logger.LogInformation($"********************userLike.SourceUserId:{userLike.SourceUserId},userLike.LikedUserId:{userLike.LikedUserId}********************");

            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync().ConfigureAwait(false))
            {
                return Ok();
            }

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetLikes(string predicate)
        {
            _logger.LogInformation($"********************predicate:{predicate}********************");

            var users = await _likesRepository.GetUserLikes(predicate, User.GetUserId()).ConfigureAwait(false);
            return Ok(users);
        }
    }
}
