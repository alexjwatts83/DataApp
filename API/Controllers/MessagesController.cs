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
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize]
    [EnableCors("AllowOrigin")]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MessagesController> _logger;
        private readonly IMesageRepository _mesageRepository;
        private readonly IUserRepository _userRepository;

        public MessagesController(
            IMapper mapper,
            ILogger<MessagesController> logger,
            IMesageRepository mesageRepository,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _mesageRepository = mesageRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            
            if(username == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest($"You can't send a message to yourself");
            }
            
            var sender = await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername).ConfigureAwait(false);
            
            if(recipient == null)
            {
                return NotFound("recipient user not found");
            }

            var message = new Message()
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _mesageRepository.AddMessage(message);

            if (await _mesageRepository.SaveAllAsync().ConfigureAwait(false)) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

    }
}
