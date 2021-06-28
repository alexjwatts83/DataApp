using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Interface;
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

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> Get()
        {
            var users = await _userRepository.GetUsersAsync().ConfigureAwait(false);
            return Ok(users);
        }

        // api/users/elle
        [HttpGet("{username}")]
        public async Task<ActionResult<AppUser>> GetById(string username)
        {
            return await _userRepository.GerUserByUsernameAsync(username).ConfigureAwait(false);
        }
    }
}
