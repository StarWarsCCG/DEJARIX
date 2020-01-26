using System;
using System.Threading;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Dejarix.App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dejarix.App.Controllers
{
    public class GameController : Controller
    {
        private readonly DejarixDbContext _context;
        private readonly UserManager<DejarixUser> _userManager;

        public GameController(
            DejarixDbContext context,
            UserManager<DejarixUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("game/{gameId}")]
        public async Task<IActionResult> GetGame(
            Guid gameId,
            CancellationToken cancellationToken)
        {
            var game = await _context.Games.FindAsync(gameId);

            if (game is null)
            {
                return NotFound();
            }
            else
            {
                var model = new GameViewModel
                {
                    Game = game,
                    Spectating = true
                };

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);

                    
                }
                else if (!game.AllowSpectators)
                {
                    return Unauthorized();
                }

                return View("Game", model);
            }
        }
    }
}