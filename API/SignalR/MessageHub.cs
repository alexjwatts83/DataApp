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
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;

        public MessageHub(
            IMessageRepository messageRepository,
            IMapper mapper,
            ILogger<MessageHub> logger,
            IUserRepository userRepository,
            IHubContext<PresenceHub> presenceHub,
            PresenceTracker presenceTracker)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            //_logger.LogInformation($"============== otherUser: '{otherUser}'");
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            //_logger.LogInformation($"============== groupName: '{groupName}'");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);

            var group = await AddToGroup(groupName).ConfigureAwait(false);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group).ConfigureAwait(false);

            var messages = await _messageRepository.GetMessageThreadAsync(Context.User.GetUsername(), otherUser).ConfigureAwait(false);
            await Clients.Caller.SendAsync("RecievedMessageThread", messages).ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup().ConfigureAwait(false);
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group).ConfigureAwait(false);
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
            else
            {
                _logger.LogInformation($"SendMessage ============== : recipient.UserName'{recipient.UserName}'");
                var connections = await _presenceTracker.GetConnectionsForUserAsync(recipient.UserName);
                _logger.LogInformation($"SendMessage ============== : connections.Count'{connections.Count}'");
                if(connections != null)
                {
                    _logger.LogInformation($"SendMessage ============== : {recipient.UserName} is online but not connected to the same group");
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
                    _logger.LogInformation($"SendMessage ============== : Sent message to client");
                }
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

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageRepository.GetMessageGroupAsync(groupName).ConfigureAwait(false);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if(group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            if(await _messageRepository.SaveAllAsync().ConfigureAwait(false))
            {
                return group;
            }

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);
            if (await _messageRepository.SaveAllAsync().ConfigureAwait(false))
            {
                return group;
            }

            throw new HubException("Failed to remove from group");
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
