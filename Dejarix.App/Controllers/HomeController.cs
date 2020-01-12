using System.Diagnostics;
using Dejarix.App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Dejarix.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")] public IActionResult Index() => View();
        [HttpGet("privacy")] public IActionResult Privacy() => View();
        
        [HttpGet("status-code/{statusCode}")]
        public IActionResult StatusCodeGet(int statusCode)
        {
            if (400 <= statusCode && statusCode < 600)
            {
                var model = new StatusCodeViewModel
                {
                    StatusCode = statusCode,
                    ReasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode)
                };

                HttpContext.Response.StatusCode = statusCode;
                return View("StatusCode", model);
            }
            else
            {
                return Redirect("/");
            }
        }

        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}