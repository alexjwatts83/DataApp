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
using Microsoft.Extensions.Logging;

namespace API.Data
{
    public class MesageRepository : IMesageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MesageRepository> _logger;

        public MesageRepository(DataContext context,
                                IMapper mapper,
                                ILogger<MesageRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
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

        public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            var query = _context
                .Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container.ToLower() switch
            {
                "inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && !u.RecipientDeleted),
                "outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && !u.SenderDeleted),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username && !u.DateRead.HasValue && !u.RecipientDeleted)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>
                .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
        {
            var messages = await _context
                .Messages
                .Include(x => x.Sender).ThenInclude(x => x.Photos)
                .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                .Where(x => (x.Recipient.UserName == currentUsername && !x.RecipientDeleted && x.Sender.UserName == recipientUsername)
                    || (x.Sender.UserName == currentUsername && !x.SenderDeleted && x.Recipient.UserName == recipientUsername))
                .OrderBy(x => x.MessageSent)
                .ToListAsync()
                .ConfigureAwait(false);

            var unreadMessages = messages
                .Where(x => !x.DateRead.HasValue && x.Recipient.UserName == currentUsername)
                .ToList();

            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages)
                {
                    message.DateRead = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
    }
}
