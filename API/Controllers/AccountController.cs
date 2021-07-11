using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContext context, ILogger<AccountController> logger, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        [EnableCors("AllowOrigin")]
        public async Task<ActionResult<UserDto>> Register(RegisterUserDto registerUserDto)
        {
            if (await UserExists(registerUserDto.Username).ConfigureAwait(false))
                return BadRequest("Username exists");

            var user = _mapper.Map<AppUser>(registerUserDto);

            user.UserName = registerUserDto.Username.ToLower();

            _context.Users.Add(user);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return new UserDto {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        [EnableCors("AllowOrigin")]
        public async Task<ActionResult<UserDto>> Login(LoginUserDto loginUserDto)
        {
            var user = await _context.Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginUserDto.Username.ToLower());

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.GetMainPhotoUrl(),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower()).ConfigureAwait(false);
        }
    }
}
