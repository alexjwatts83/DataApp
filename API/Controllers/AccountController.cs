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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(
            ILogger<AccountController> logger,
            ITokenService tokenService,
            IMapper mapper,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _tokenService = tokenService;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        [EnableCors("AllowOrigin")]
        public async Task<ActionResult<UserDto>> Register(RegisterUserDto registerUserDto)
        {
            if (await UserExistsAsync(registerUserDto.Username).ConfigureAwait(false))
                return BadRequest("Username exists");

            var user = _mapper.Map<AppUser>(registerUserDto);

            user.UserName = registerUserDto.Username.ToLower();

            var result = await _userManager.CreateAsync(user, registerUserDto.Password);

            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

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
            var user = await _userManager.Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginUserDto.Username.ToLower())
                .ConfigureAwait(false);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, loginUserDto.Password, false)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return Unauthorized();
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

        private async Task<bool> UserExistsAsync(string username)
        {
            return await _userManager
                .Users
                .AnyAsync(x => x.UserName == username.ToLower())
                .ConfigureAwait(false);
        }
    }
}
