using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize]
    [EnableCors("AllowOrigin")]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger, IMapper mapper)
        {
            _userRepository = userRepository;
            _logger = logger;
            _mapper = mapper;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> Get()
        {
            var users = await _userRepository.GetUsersAsync().ConfigureAwait(false);
            var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(memberDtos);
        }

        // api/users/elle
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetById(string username)
        {
            var user = await _userRepository.GerUserByUsernameAsync(username).ConfigureAwait(false);
            var memberDto = _mapper.Map<MemberDto>(user);
            return memberDto;
        }
    }
}
