using System;
using System.Threading.Tasks;
using API.Extentions;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace API.SignalR
{

    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(IMessageRepository messageRepository, IMapper mapper, ILogger<MessageHub> logger)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);
            var messages = await _messageRepository.GetMessageThreadAsync(Context.User.GetUsername(), otherUser).ConfigureAwait(false);
            await Clients.Group(groupName).SendAsync("RecievedMessageThread", messages).ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
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
