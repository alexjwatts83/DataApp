using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [EnableCors("AllowOrigin")]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> Get()
        {
            var memberDtos = await _userRepository.GetMembersAsync().ConfigureAwait(false);
            return Ok(memberDtos);
        }

        // api/users/elle
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetByUsername(string username)
        {
            return await _userRepository.GetMemberAsync(username).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GerUserByUsernameAsync(username).ConfigureAwait(false);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if(await _userRepository.SallAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }
    }
}
