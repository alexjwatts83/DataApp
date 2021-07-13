using System;
using System.Threading.Tasks;
using API.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId).ConfigureAwait(false);
            if(isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername()).ConfigureAwait(false);
            }
            

            var currentUsers = await _presenceTracker.GetOnlineUsers();

            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers).ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId).ConfigureAwait(false);
            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername()).ConfigureAwait(false);
            }

            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
    }
}
