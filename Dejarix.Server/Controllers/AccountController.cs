using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dejarix.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Dejarix.Server.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<DejarixUser> _userManager;
        private readonly Mailgun _mailgun;

        public AccountController(
            UserManager<DejarixUser> userManager,
            Mailgun mailgun)
        {
            _userManager = userManager;
            _mailgun = mailgun;
        }

        [HttpPost("Register")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPost()
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var user = new DejarixUser
            {
                Id = Guid.NewGuid(),
                UserName = formData["register-username"],
                Email = formData["register-email"],
                RegistrationDate = DateTimeOffset.Now
            };

            var password = formData["register-password"];
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return Ok(token);
            }
            else
            {
                ViewData["RegistrationErrors"] = result.Errors
                    .Select(e => e.Description)
                    .ToArray();

                return View("Register");
            }
        }

        [HttpGet("Register")]
        public IActionResult RegisterGet()
        {
            return View("Register");
        }

        [HttpGet("Verify/{token}")]
        public IActionResult Verify(string token)
        {
            return Ok(token);
        }
    }
}