using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
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
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessagesController(
            IMapper mapper,
            ILogger<MessagesController> logger,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            // TODO: figure out a way to make this generic so messagehub and messagecontroller both use the same function
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

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync().ConfigureAwait(false)) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesFor([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await _messageRepository.GetMessagesForUserAsync(messageParams).ConfigureAwait(false);

            Response.AddPaginationHeader(
                messages.CurrentPage,
                messages.PageSize,
                messages.TotalCount,
                messages.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThreadAsync(string username)
        {
            var currentUsername = User.GetUsername();
            var messages = await _messageRepository
                .GetMessageThreadAsync(currentUsername, username)
                .ConfigureAwait(false);

            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _messageRepository.GetMessageAsync(id).ConfigureAwait(false);
            
            if(message == null)
            {
                return BadRequest("Message not found, could not delete");
            }

            if(message.Sender.UserName != username && message.Recipient.UserName != username)
            {
                return Unauthorized();
            }

            if(message.Sender.UserName == username)
            {
                message.SenderDeleted = true;
            }

            if(message.Recipient.UserName == username)
            {
                message.RecipientDeleted = true;
            }

            if(message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if (await _messageRepository.SaveAllAsync().ConfigureAwait(false))
            {
                return Ok();
            }

            return BadRequest("Failed to send message");
        }
    }
}
