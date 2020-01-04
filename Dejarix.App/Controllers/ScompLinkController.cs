using Microsoft.AspNetCore.Mvc;

namespace Dejarix.App.Controllers
{
    [Route("scomp-link")]
    [ApiController]
    public class ScompLinkController : ControllerBase
    {
        [HttpGet("test")]
        [Produces("application/json")]
        public IActionResult Test()
        {
            return Ok(new int[] {1, 1, 3, 8});
        }
    }
}