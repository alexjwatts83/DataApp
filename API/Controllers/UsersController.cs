﻿using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(DataContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // api/users
        [HttpGet]
        [EnableCors("AllowOrigin")]
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