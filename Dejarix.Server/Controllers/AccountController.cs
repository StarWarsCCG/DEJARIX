using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dejarix.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Dejarix.Server.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<DejarixUser> _userManager;
        private readonly SignInManager<DejarixUser> _signInManager;
        private readonly Mailgun _mailgun;

        public AccountController(
            UserManager<DejarixUser> userManager,
            SignInManager<DejarixUser> signInManager,
            Mailgun mailgun)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            var passwordConfirm = formData["register-password-confirm"];
            if (password != passwordConfirm)
            {
                ViewData["RegistrationErrors"] = new string[]{"Passwords do not match."};
                return View("Register");
            }

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var bytes = Encoding.UTF8.GetBytes(token);
                var hexToken = bytes.ToHex();
                var url = $"http://localhost:5000/Account/Confirm?userId={user.Id}&token={hexToken}";
                await _mailgun.SendEmailAsync(
                    "DEJARIX Admin <admin@obviouspiranha.com>",
                    user.Email.Yield(),
                    null,
                    null,
                    $"Confirm {user.Email} on DEJARIX",
                    "Visit this URL to confirm you email address at DEJARIX: " + url,
                    $"<h1>DEJARIX Registration</h1><p>Click <a href='{url}'>here</a> to confirm your email address.</p>");
                
                ViewData["RegistrationSuccess"] = $"Registration successful! Check {user.Email} for your confirmation link!";
                return View("Register");
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

        [HttpPost("SignIn")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInPost()
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var signInUser = formData["sign-in-user"];
            var signInPass = formData["sign-in-pass"];
            var user = await _userManager.FindByNameAsync(signInUser);
            var passwordResult = await _signInManager.CheckPasswordSignInAsync(
                user, signInPass, false);
            
            if (passwordResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, true);
                return Redirect("/");
            }
            else
            {
                ViewData["SignInError"] = "Failed to sign in.";
                return View("SignIn");
            }
        }

        [HttpGet("SignIn")]
        public IActionResult SignInGet()
        {
            return View("SignIn");
        }

        [HttpPost("SignOut")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet("Confirm")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Confirm(Guid userId, string token)
        {
            var bytes = token.AsHex();
            var trueToken = Encoding.UTF8.GetString(bytes);

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return Redirect("/");

            var result = await _userManager.ConfirmEmailAsync(user, trueToken);

            if (result.Succeeded)
            {
                ViewData["ConfirmedUser"] = user;
                return View();
            }

            return Redirect("/");
        }
    }
}