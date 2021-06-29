using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Interface;
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

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
    }
}
