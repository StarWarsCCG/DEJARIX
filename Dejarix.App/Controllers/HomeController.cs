using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dejarix.App.Models;
using Microsoft.AspNetCore.Identity;
using Dejarix.App.Entities;
using System.Text;

namespace Dejarix.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<DejarixUser> _userManager;
        private readonly SignInManager<DejarixUser> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<DejarixUser> userManager,
            SignInManager<DejarixUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("")] public IActionResult Index() => View();
        [HttpGet("privacy")] public IActionResult Privacy() => View();
        
        [HttpGet("sign-in")]
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect("/");
            else
                return View();
        }

        [HttpPost("sign-in")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInPost()
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var signInUser = formData["sign-in-user"].First();
            var signInPass = formData["sign-in-pass"].First();
            var user = signInUser.Contains('@') ?
                await _userManager.FindByEmailAsync(signInUser) :
                await _userManager.FindByNameAsync(signInUser);
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
                return View(nameof(SignIn));
            }
        }

        [HttpPost("sign-out")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
        
        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect("/");
            else
                return View();
        }

        [HttpPost("register")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPost([FromServices] Mailgun mailgun)
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
                return View(nameof(Register));
            }

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var bytes = Encoding.UTF8.GetBytes(token);
                var hexToken = bytes.ToHex();
                var host = request.Scheme + "://" + request.Host;
                var path = Url.Action(nameof(Confirm), new { userId = user.Id, token = hexToken });
                var url = host + path;
                var email = new Email
                {
                    From = mailgun.DefaultSender,
                    To = new string[]{user.Email},
                    Bcc = mailgun.DefaultBcc,
                    Subject = $"Confirm {user.Email} on DEJARIX",
                    TextBody = "Visit this URL to confirm your email address at DEJARIX: " + url,
                    HtmlBody = $"<h2>DEJARIX Registration</h2><p>Click <a href='{url}'>here</a> to confirm your email address.</p>"
                };

                var response = await mailgun.SendEmailAsync(email);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                
                ViewData["RegistrationSuccess"] = $"Registration successful! Check {user.Email} for your confirmation link!";
                return View(nameof(Register));
            }
            else
            {
                ViewData["RegistrationErrors"] = result.Errors
                    .Select(e => e.Description)
                    .ToArray();

                return View(nameof(Register));
            }
        }

        [HttpGet("confirm")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Confirm(Guid userId, string token)
        {
            var bytes = token.AsHex();
            var trueToken = Encoding.UTF8.GetString(bytes);

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return Redirect("/");

            var result = await _userManager.ConfirmEmailAsync(user, trueToken);

            if (result.Succeeded)
            {
                ViewData["ConfirmedUser"] = user;
                return View();
            }

            return Redirect("/");
        }

        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
