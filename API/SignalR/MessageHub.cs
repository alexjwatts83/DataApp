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
        private readonly IMapper _mapper;
        private readonly ILogger<MessageHub> _logger;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;
        private readonly IUnitOfWork _unitOfWork;

        public MessageHub(
            IMapper mapper,
            ILogger<MessageHub> logger,
            IHubContext<PresenceHub> presenceHub,
            PresenceTracker presenceTracker,
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _logger = logger;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
            _unitOfWork = unitOfWork;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);

            var group = await AddToGroup(groupName).ConfigureAwait(false);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group).ConfigureAwait(false);

            var messages = await _unitOfWork.MessageRepository.GetMessageThreadAsync(Context.User.GetUsername(), otherUser).ConfigureAwait(false);

            if(_unitOfWork.HasChanged())
            {
                await _unitOfWork.Complete();
            }
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

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername).ConfigureAwait(false);

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
            var group = await _unitOfWork.MessageRepository.GetMessageGroupAsync(groupName).ConfigureAwait(false);

            if(group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _presenceTracker.GetConnectionsForUserAsync(recipient.UserName).ConfigureAwait(false);
                if(connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    }).ConfigureAwait(false);
                }
            }
            
            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete().ConfigureAwait(false))
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message)).ConfigureAwait(false);
            }
            else
            {
                throw new HubException("Failed to send message");
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroupAsync(groupName).ConfigureAwait(false);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if(group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            if(await _unitOfWork.Complete().ConfigureAwait(false))
            {
                return group;
            }

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await _unitOfWork.Complete().ConfigureAwait(false))
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
