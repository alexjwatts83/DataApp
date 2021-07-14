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

            var userId = resultContenxt.HttpContext.User.GetUserId();
            var unitOfWork = resultContenxt.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId).ConfigureAwait(false);

            user.LastActive = DateTime.UtcNow;

            await unitOfWork.Complete().ConfigureAwait(false);
        }
    }
}
