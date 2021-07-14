using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();
        private readonly ILogger<PresenceTracker> _logger;
        public PresenceTracker(ILogger<PresenceTracker> logger)
        {
            _logger = logger;
        }

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock(OnlineUsers)
            {
                if(OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                } else
                {
                    OnlineUsers.Add(username, new List<string>() { connectionId });
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username))
                {
                    return Task.FromResult(isOffline);
                }

                OnlineUsers[username].Remove(connectionId);

                if(OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock(OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }
            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUserAsync(string username)
        {
            _logger.LogInformation("PresenceTracker ===================== calling GetConnectionsForUserAsync");
            _logger.LogInformation("PresenceTracker ===================== username:"  + username);
            _logger.LogInformation("PresenceTracker ===================== OnlineUsers:" + (OnlineUsers == null));
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                _logger.LogInformation("PresenceTracker ===================== in lock");
                _logger.LogInformation("PresenceTracker ===================== in lock");
                connectionIds = OnlineUsers.GetValueOrDefault(username);
                _logger.LogInformation("PresenceTracker ===================== out of lock");
                _logger.LogInformation("PresenceTracker ===================== connectionIds is null " + ((connectionIds == null) ? "null af" : connectionIds.Count.ToString()));
            }
            if(connectionIds == null)
            {
                connectionIds = new List<string>();
            }
            _logger.LogInformation("PresenceTracker ===================== unlocked");
            return Task.FromResult(connectionIds);
        }
    }
}
