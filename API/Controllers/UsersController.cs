using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<IEnumerable<AppUser>> Get()
        {
            return _context.Users.ToList();
        }

        // api/users/1
        [HttpGet("{id}")]
        public ActionResult<AppUser> GetById(int id)
        {
            return _context.Users.Find(id);
        }
    }
}
