using System;
using System.Threading.Tasks;
using API.Extentions;
using API.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContenxt = await next().ConfigureAwait(false);

            if(!resultContenxt.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }

            var username = resultContenxt.HttpContext.User.GetUsername();
            var respository = resultContenxt.HttpContext.RequestServices.GetService<IUserRepository>();
            var user = await respository.GerUserByUsernameAsync(username).ConfigureAwait(false);

            user.LastActive = DateTime.Now;

            await respository.SallAllAsync();
        }
    }
}
