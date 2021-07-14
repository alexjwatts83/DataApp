using System.Threading.Tasks;
using API.Interface;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public UnitOfWork(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }
        public IUserRepository UserRepository => new UserRepository(_dataContext, _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_dataContext, _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_dataContext, _mapper);

        public async Task<bool> Complete()
        {
            return await _dataContext.SaveChangesAsync().ConfigureAwait(false) > 0;
        }

        public bool HasChanged()
        {
            return _dataContext.ChangeTracker.HasChanges();
        }
    }
}
