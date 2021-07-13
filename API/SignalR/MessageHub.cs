using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageHub> _logger;
        private readonly IUserRepository _userRepository;

        public MessageHub(
            IMessageRepository messageRepository,
            IMapper mapper,
            ILogger<MessageHub> logger,
            IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            _logger.LogInformation($"============== otherUser: '{otherUser}'");
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            _logger.LogInformation($"============== groupName: '{groupName}'");

            await AddToGroup(Context, groupName).ConfigureAwait(false);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);

            var messages = await _messageRepository.GetMessageThreadAsync(Context.User.GetUsername(), otherUser).ConfigureAwait(false);
            await Clients.Group(groupName).SendAsync("RecievedMessageThread", messages).ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            // TODO: figure out a way to make this generic so messagehub and messagecontroller both use the same function
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException($"You can't send a message to yourself, {username}");
            }

            var sender = await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername).ConfigureAwait(false);

            if (recipient == null)
            {
                throw new HubException($"recipient user, {createMessageDto.RecipientUsername}, not found");
            }

            var message = new Message()
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _messageRepository.GetMessageGroupAsync(groupName).ConfigureAwait(false);
            if(group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync().ConfigureAwait(false))
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message)).ConfigureAwait(false);
            }else
            {
                throw new HubException("Failed to send message");
            }
        }

        private async Task<bool> AddToGroup(HubCallerContext context, string groupName)
        {
            var group = await _messageRepository.GetMessageGroupAsync(groupName).ConfigureAwait(false);
            var connection = new Connection(context.ConnectionId, context.User.GetUsername());
            if(group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            return await _messageRepository.SaveAllAsync().ConfigureAwait(false);
        }

        private async Task RemoveFromMessageGroup(string connectionId)
        {
            var connection = await _messageRepository.GetConnectionAsync(connectionId).ConfigureAwait(false);
            _messageRepository.RemoveConnection(connection);
            await _messageRepository.SaveAllAsync().ConfigureAwait(false);
        }

        private static string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare
                ? $"{caller}-{other}"
                : $"{other}-{caller}";

        }
    }
}
