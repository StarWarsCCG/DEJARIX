using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Dejarix.App.Entities;
using System.Text;
using Dejarix.App.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Dejarix.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<DejarixUser> _userManager;
        private readonly SignInManager<DejarixUser> _signInManager;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<DejarixUser> userManager,
            SignInManager<DejarixUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("sign-in")]
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(HomeController.Index), "Home");
            else
                return View(new SignInViewModel());
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
            bool isEmail = signInUser.Contains('@');
            var user = isEmail ?
                await _userManager.FindByEmailAsync(signInUser) :
                await _userManager.FindByNameAsync(signInUser);
            
            if (user is null)
            {
                var word = isEmail ? "email" : "user";
                var model = new SignInViewModel
                {
                    PreviousUserName = signInUser,
                    Error = $"Unrecognized {word}: {signInUser}"
                };

                return View(nameof(SignIn), model);
            }

            var passwordResult = await _signInManager.CheckPasswordSignInAsync(
                user, signInPass, false);
            
            if (passwordResult.Succeeded)
            {
                var signInPersist = formData["sign-in-persist"].FirstOrDefault();
                await _signInManager.SignInAsync(user, signInPersist == "on");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                var model = new SignInViewModel
                {
                    PreviousUserName = signInUser,
                    Error = "Incorrect password."
                };

                return View(nameof(SignIn), model);
            }
        }

        [HttpPost("sign-out")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        
        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(HomeController.Index), "Home");
            else
                return View(new RegisterViewModel());
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

            var model = new RegisterViewModel
            {
                PreviousUserName = user.UserName,
                PreviousEmail = user.Email
            };

            var errors = new List<string>();
            if (user.UserName.Length < 3)
                errors.Add("Username must be at least 3 characters.");

            var password = formData["register-password"];
            var passwordConfirm = formData["register-password-confirm"];
            if (password != passwordConfirm)
                errors.Add("Passwords do not match.");
            
            if (errors.Count > 0)
            {
                model.Errors = errors.ToArray();
                return View(nameof(Register), model);
            }

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var bytes = Encoding.UTF8.GetBytes(token);
                var hexToken = bytes.ToHex();

                // https://stackoverflow.com/a/38311283
                var path = Url.Action(nameof(Confirm), new { userId = user.Id, token = hexToken });
                var host = request.Scheme + "://" + request.Host;
                var url = host + path;
                var email = new Email
                {
                    From = mailgun.DefaultSender,
                    To = ImmutableArray.Create(user.Email),
                    Bcc = mailgun.DefaultBcc,
                    Subject = $"Confirm {user.Email} on DEJARIX",
                    TextBody = "Visit this URL to confirm your email address at DEJARIX: " + url,
                    HtmlBody = $"<h2>DEJARIX Registration</h2><p>Click <a href='{url}'>here</a> to confirm your email address.</p>"
                };

                var response = await mailgun.SendEmailAsync(email);
                model.Success = $"Registration successful! Check {user.Email} for your confirmation link! (Remember to check your spam folder.)";
                return View(nameof(Register), model);
            }
            else
            {
                model.Errors = result.Errors
                    .Select(e => e.Description)
                    .ToArray();

                return View(nameof(Register), model);
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
                return RedirectToAction(nameof(HomeController.Index), "Home");

            var result = await _userManager.ConfirmEmailAsync(user, trueToken);

            if (result.Succeeded)
            {
                var model = new ConfirmViewModel
                {
                    UserName = user.UserName,
                    UserEmail = user.Email
                };

                return View(model);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet("forgot")]
        public IActionResult Forgot()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(HomeController.Index), "Home");
            
            return View(new ForgotViewModel());
        }

        [HttpPost("forgot")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPost([FromServices] Mailgun mailgun)
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var forgotUser = formData["forgot-user"].FirstOrDefault() ?? string.Empty;

            bool isEmail = forgotUser.Contains('@');
            var user = isEmail ?
                await _userManager.FindByEmailAsync(forgotUser) :
                await _userManager.FindByNameAsync(forgotUser);
            
            var model = new ForgotViewModel { PreviousUserName = forgotUser };
            
            if (user is null)
            {
                var word = isEmail ? "email" : "user";
                model.Error = $"Unable to find user associated with '{forgotUser}'.";
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var bytes = Encoding.UTF8.GetBytes(token);
                var hexToken = bytes.ToHex();
                // https://stackoverflow.com/a/38311283
                var path = Url.Action(nameof(Reset), new { userId = user.Id, token = hexToken });
                var host = request.Scheme + "://" + request.Host;
                var url = host + path;
                var email = new Email
                {
                    From = mailgun.DefaultSender,
                    To = ImmutableArray.Create(user.Email),
                    Bcc = mailgun.DefaultBcc,
                    Subject = $"DEJARIX - Reset password for {user.UserName}",
                    TextBody = "Visit this URL to reset your password at DEJARIX: " + url,
                    HtmlBody = $"<h2>DEJARIX Password Reset</h2><p>Click <a href='{url}'>here</a> to reset your password.</p>"
                };
                
                var response = await mailgun.SendEmailAsync(email);
                model.Success = "An email has been sent!";
            }

            return View("Forgot", model);
        }

        [HttpGet("reset")]
        public IActionResult Reset(Guid userId, string token)
        {
            var bytes = token.AsHex();
            var trueToken = Encoding.UTF8.GetString(bytes);

            var model = new ResetViewModel
            {
                UserId = userId,
                Token = trueToken
            };

            return View(model);
        }

        [HttpPost("reset")]
        [Consumes("application/x-www-form-urlencoded")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPost()
        {
            var request = HttpContext.Request;
            var formData = await request.ReadFormAsync();
            var textUserId = formData["reset-user-id"].FirstOrDefault();
            var token = formData["reset-token"].FirstOrDefault();
            var password = formData["reset-password"].FirstOrDefault();
            var confirmPassword = formData["reset-password-confirm"].FirstOrDefault();

            var model = new ResetViewModel
            {
                UserId = Guid.Parse(textUserId),
                Token = token
            };

            if (password != confirmPassword)
            {
                model.Errors = new string[]{"Passwords do not match."};
            }
            else
            {
                var user = await _userManager.FindByIdAsync(textUserId);

                if (user is null)
                {
                    model.Errors = new string[]{"Unrecognized user ID."};
                }
                else
                {
                    var result = await _userManager.ResetPasswordAsync(user, token, password);

                    if (result.Succeeded)
                    {
                        model.Success = "Password successfully reset!";
                    }
                    else
                    {
                        model.Errors = result.Errors.Select(e => e.Description).ToArray();
                    }
                }
            }
            
            return View("Reset", model);
        }
    }
}
