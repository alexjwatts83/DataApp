using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context,
                                IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnectionAsync(string connectionId)
        {
            return await _context
                .Connections
                .FindAsync(connectionId)
                .ConfigureAwait(false);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(x => x.Connections)
                                        .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                                        .FirstOrDefaultAsync()
                                        .ConfigureAwait(false);
        }

        public async Task<Message> GetMessageAsync(int id)
        {
            return await _context
                .Messages
                .Include(x => x.Recipient)
                .Include(x => x.Sender)
                .SingleOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<Group> GetMessageGroupAsync(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections)
                                        .FirstOrDefaultAsync(x => x.Name == groupName)
                                        .ConfigureAwait(false);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            var query = _context
                .Messages
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container.ToLower() switch
            {
                "inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && !u.RecipientDeleted),
                "outbox" => query.Where(u => u.SenderUsername == messageParams.Username && !u.SenderDeleted),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && !u.DateRead.HasValue && !u.RecipientDeleted)
            };

            return await PagedList<MessageDto>
                .CreateAsync(query, messageParams.PageNumber, messageParams.PageSize)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
        {
            var messages = await _context
                .Messages
                .Where(x => (x.Recipient.UserName == currentUsername && !x.RecipientDeleted && x.Sender.UserName == recipientUsername)
                    || (x.Sender.UserName == currentUsername && !x.SenderDeleted && x.Recipient.UserName == recipientUsername))
                .OrderBy(x => x.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync()
                .ConfigureAwait(false);

            var unreadMessages = messages
                .Where(x => !x.DateRead.HasValue && x.RecipientUsername == currentUsername)
                .ToList();

            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}
