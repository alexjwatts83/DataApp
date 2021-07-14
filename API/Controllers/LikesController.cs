using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
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
        private readonly ILogger<LikesController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public LikesController(ILogger<LikesController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();

            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
            var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId).ConfigureAwait(false);

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

            var userLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id).ConfigureAwait(false);

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

            if (await _unitOfWork.Complete().ConfigureAwait(false))
            {
                return Ok();
            }

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetLikes([FromQuery]LikesParams likesParams)
        {
            //_logger.LogInformation($"********************predicate:{predicate}********************");
            likesParams.UserId = User.GetUserId();

            var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams).ConfigureAwait(false);

            Response.AddPaginationHeader(
                users.CurrentPage,
                users.PageSize,
                users.TotalCount,
                users.TotalPages);

            return Ok(users);
        }
    }
}
