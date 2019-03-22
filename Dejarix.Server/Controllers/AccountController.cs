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
using Microsoft.Extensions.Configuration;

namespace Dejarix.Server.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<DejarixUser> _userManager;
        private readonly SignInManager<DejarixUser> _signInManager;
        private readonly Mailgun _mailgun;
        private readonly string _linkHost;
        private readonly string _sender;
        private readonly string _bcc;

        public AccountController(
            IConfiguration configuration,
            UserManager<DejarixUser> userManager,
            SignInManager<DejarixUser> signInManager,
            Mailgun mailgun)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailgun = mailgun;

            _linkHost = configuration["LinkHost"];
            _sender = configuration["SenderEmailAddress"];
            _bcc = configuration["BccEmailAddress"];
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
                var url = $"{_linkHost}/Account/Confirm?userId={user.Id}&token={hexToken}";
                var response = await _mailgun.SendEmailAsync(
                    _sender,
                    user.Email.Yield(),
                    null,
                    _bcc?.Yield(),
                    $"Confirm {user.Email} on DEJARIX",
                    "Visit this URL to confirm your email address at DEJARIX: " + url,
                    $"<h2>DEJARIX Registration</h2><p>Click <a href='{url}'>here</a> to confirm your email address.</p>");
                
                response.EnsureSuccessStatusCode();
                
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

        [HttpPost("Forgot")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPost()
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var forgotUser = formData["forgot-user"].FirstOrDefault() ?? string.Empty;
            var user = forgotUser.Contains('@') ?
                await _userManager.FindByEmailAsync(forgotUser) :
                await _userManager.FindByNameAsync(forgotUser);
            
            if (user == null)
            {
                ViewData["ForgotErrors"] = new string[]{$"Unable to find user associated with '{forgotUser}'."};
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var bytes = Encoding.UTF8.GetBytes(token);
                var hexToken = bytes.ToHex();
                var url = $"{_linkHost}/Account/Reset?userId={user.Id}&token={hexToken}";
                var response = await _mailgun.SendEmailAsync(
                    _sender,
                    user.Email.Yield(),
                    null,
                    _bcc?.Yield(),
                    $"DEJARIX - Reset password for {user.UserName}",
                    "Visit this URL to reset your password at DEJARIX: " + url,
                    $"<h2>DEJARIX Password Reset</h2><p>Click <a href='{url}'>here</a> to reset your password.</p>");
                
                response.EnsureSuccessStatusCode();
                
                ViewData["ForgotSuccess"] = "An email has been sent!";
            }

            return View("Forgot");
        }

        [HttpGet("Forgot")]
        public IActionResult ForgotGet()
        {
            return View("Forgot");
        }

        [HttpPost("Reset")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPost()
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var userId = formData["reset-user-id"].FirstOrDefault();
            var token = formData["reset-token"].FirstOrDefault();
            ViewData["UserId"] = userId;
            ViewData["Token"] = token;
            var password = formData["reset-password"].FirstOrDefault();
            var confirmPassword = formData["reset-password-confirm"].FirstOrDefault();

            if (password != confirmPassword)
            {
                ViewData["ResetErrors"] = new string[]{"Passwords do not match."};
            }
            else
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    ViewData["ResetErrors"] = new string[]{"Unrecognized user ID."};
                }
                else
                {
                    var result = await _userManager.ResetPasswordAsync(user, token, password);

                    if (result.Succeeded)
                    {
                        ViewData["ResetSuccess"] = "Password successfully reset!";
                    }
                    else
                    {
                        ViewData["ResetErrors"] = result.Errors.Select(e => e.Description).ToArray();
                    }
                }
            }
            
            return View("Reset");
        }

        [HttpGet("Reset")]
        public IActionResult ResetGet(Guid userId, string token)
        {
            var bytes = token.AsHex();
            var trueToken = Encoding.UTF8.GetString(bytes);
            ViewData["UserId"] = userId.ToString();
            ViewData["Token"] = trueToken;
            return View("Reset");
        }
    }
}