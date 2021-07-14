using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [EnableCors("AllowOrigin")]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IMapper mapper, IPhotoService photoService, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _photoService = photoService;
            _unitOfWork = unitOfWork;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> Get([FromQuery] UserParams userParams)
        {
            var username = User.GetUsername();
            var gender = await _unitOfWork.UserRepository.GetUserGender(username).ConfigureAwait(false);
            userParams.CurrentUsername = username;
            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = (string.Equals(gender, "male", System.StringComparison.OrdinalIgnoreCase)) ? "female" : "male";
            }

            var memberDtos = await _unitOfWork.UserRepository.GetMembersAsync(userParams).ConfigureAwait(false);

            Response.AddPaginationHeader(
                memberDtos.CurrentPage,
                memberDtos.PageSize,
                memberDtos.TotalCount,
                memberDtos.TotalPages);

            return Ok(memberDtos);
        }

        // api/users/elle
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetByUsername(string username)
        {
            return await _unitOfWork.UserRepository.GetMemberAsync(username).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);

            _mapper.Map(memberUpdateDto, user);

            _unitOfWork.UserRepository.Update(user);

            if(await _unitOfWork.Complete().ConfigureAwait(false))
            {
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            
            if(photo == null)
            {
                return NotFound($"Cannot find photo with id of '{photoId}'");
            }

            if(photo.IsMain)
            {
                return BadRequest($"Photo with id of '{photoId}' is already the main photo");
            }

            var photoMain = user.GetMainPhoto();

            if (photo != null)
            {
                photoMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await _unitOfWork.Complete().ConfigureAwait(false))
            {
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            if(file == null)
            {
                return BadRequest("No file lols");
            }
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);
            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if(await _unitOfWork.Complete())
            {
                return CreatedAtRoute("GetUser", new { username = User.GetUsername() }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Failed to add photo to user, lols");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
            {
                return NotFound($"Cannot find photo with id of '{photoId}'");
            }

            if (photo.IsMain)
            {
                return BadRequest("You cannot delete your main photo");
            }

            if (photo.PublicId == null)
            {
                return BadRequest("Cannot delete the very first image because I said so");
            }

            var result = await _photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _unitOfWork.Complete().ConfigureAwait(false))
            {
                return Ok();
            }

            return BadRequest("Failed to delete photo, lols");
        }
    }
}
