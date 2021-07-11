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
        public static async Task SeedUserDataAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync().ConfigureAwait(false)) return;

            var userData = await System.IO.File
                .ReadAllTextAsync("Data/UserSeedData.json")
                .ConfigureAwait(false);

            var roles = new List<AppRole>()
            {
                new AppRole()
                {
                    Name = "Member"
                },
                new AppRole()
                {
                    Name = "Admin"
                },
                new AppRole()
                {
                    Name = "Moderator"
                },
            };

            foreach (var role in roles)
            {
                await roleManager
                    .CreateAsync(role)
                    .ConfigureAwait(false);
            }

            foreach (var user in JsonSerializer.Deserialize<List<AppUser>>(userData))
            {
                user.UserName = user.UserName.ToLower();

                await userManager
                    .CreateAsync(user, "password")
                    .ConfigureAwait(false);

                await userManager
                    .AddToRoleAsync(user, "Member")
                    .ConfigureAwait(false);
            }

            var admin = new AppUser
            {
                UserName = "admin",
                KnownAs = "Administator"
            };

            await userManager
                .CreateAsync(admin, "password")
                .ConfigureAwait(false);
            await userManager
                .AddToRolesAsync(admin, new[] { "Admin", "Moderator" })
                .ConfigureAwait(false);
        }
    }
}
