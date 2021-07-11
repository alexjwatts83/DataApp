using System.Threading.Tasks;
using API.Entities;

namespace API.Interface
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser appUser);
    }
}
