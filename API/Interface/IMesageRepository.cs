using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interface
{
    public interface IMesageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessageAsync(int id);
        Task<PagedList<MessageDto>> GetMessagesForUserAsync();
        Task<IEnumerable<MessageDto>> GetMessageThreadAsync(int currentUserId, int recipientId);
        Task<bool> SaveAllAsync();
    }
}
