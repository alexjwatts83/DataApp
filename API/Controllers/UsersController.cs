using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<WeatherForecastController> _logger;

        public UsersController(DataContext context, ILogger<WeatherForecastController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> Get()
        {
            return await _context.Users.ToListAsync().ConfigureAwait(false);
        }

        // api/users/1
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetById(int id)
        {
            return await _context.Users.FindAsync(id).ConfigureAwait(false);
        }
    }
}
