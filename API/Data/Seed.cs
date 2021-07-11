using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (await userManager.Users.AnyAsync().ConfigureAwait(false)) return;

            var userData = await System.IO.File
                .ReadAllTextAsync("Data/UserSeedData.json")
                .ConfigureAwait(false);

            foreach (var user in JsonSerializer.Deserialize<List<AppUser>>(userData))
            {
                user.UserName = user.UserName.ToLower();

                await userManager
                    .CreateAsync(user, "password")
                    .ConfigureAwait(false);
            }
        }
    }
}
