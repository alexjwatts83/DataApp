using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace API.Controllers
{
    [EnableCors("AllowOrigin")]
    public class DebugController: BaseApiController
    {
        private readonly IConfiguration _configuration;

        public DebugController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("cloudinary")]
        public ActionResult<string> GetCloudinary()
        {
            var sb = new StringBuilder();
            foreach(var kp in _configuration.GetSection("CloudinarySettings").AsEnumerable())
            {
                sb.AppendLine($"{kp.Key} = '{kp.Value}'");
            }
            return Ok(sb.ToString());
        }
    }
}
