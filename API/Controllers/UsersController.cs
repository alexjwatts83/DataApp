using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
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
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> Get()
        {
            var memberDtos = await _userRepository.GetMembersAsync().ConfigureAwait(false);
            return Ok(memberDtos);
        }

        // api/users/elle
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetByUsername(string username)
        {
            return await _userRepository.GetMemberAsync(username).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepository.GerUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if(await _userRepository.SallAllAsync().ConfigureAwait(false))
            {
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GerUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            
            if(photo == null)
            {
                return NotFound($"Cannot find photo with id of '{photoId}'");
            }

            if(photo.IsMain)
            {
                return BadRequest($"Photo with id of '{photoId}' is already the main photo");
            }

            var photoMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (photo != null)
            {
                photoMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await _userRepository.SallAllAsync().ConfigureAwait(false))
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
            var user = await _userRepository.GerUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);
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

            if(await _userRepository.SallAllAsync())
            {
                return CreatedAtRoute("GetUser", new { username = User.GetUsername() }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Failed to add photo to user, lols");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GerUserByUsernameAsync(User.GetUsername()).ConfigureAwait(false);

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
            {
                return NotFound($"Cannot find photo with id of '{photoId}'");
            }

            if (photo.IsMain)
            {
                return BadRequest($"You cannot delete your main photo");
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

            if (await _userRepository.SallAllAsync().ConfigureAwait(false))
            {
                return Ok();
            }

            return BadRequest("Failed to delete photo, lols");
        }
    }
}
