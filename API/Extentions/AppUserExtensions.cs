using System.Linq;
using API.Entities;

namespace API.Extentions
{
    public static class AppUserExtensions
    {
        public static Photo GetMainPhoto(this AppUser appUser)
        {
            return appUser.Photos?.FirstOrDefault(x => x.IsMain);
        }

        public static string GetMainPhotoUrl(this AppUser appUser)
        {
            return appUser.GetMainPhoto()?.Url;
        }
    }
}
