using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPicker.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public class LoginViewModel
        {
            [Required]
            [Display(Name="User")]
            public string UserId { get; set; }
            
            [Display(Name="Password")]
            public string Password { get; set; }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model, [FromQuery] string returnUrl)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (!string.IsNullOrEmpty(model.Password) && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Password login
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    await _signInManager.SignInWithClaimsAsync(user, false,
                            new[] { new Claim("PasswordLogin", "true") });
                }
                else
                {
                    ModelState.AddModelError("Password", "Invalid password");
                    return View();
                }
            }
            else
            {
                // Normal login
                await _signInManager.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = true
                });
            }
            
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        
        public class RegisterViewModel
        {
            [Required]
            public string Username { get; set; }
            
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            if (_userManager.Users.Any()) return Unauthorized();

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (_userManager.Users.Any()) return Unauthorized();
            if (!ModelState.IsValid)
            {
                return View();
            }
            var createResult = await _userManager.CreateAsync(new ApplicationUser()
            {
                UserName = model.Username,
                VoteIsRequired = true,
            }, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var identityError in createResult.Errors)
                {
                    ModelState.AddModelError("Password", identityError.Description);
                    return View();
                }

                _logger.LogError("Error creating user: {Error}", string.Join(", ", createResult.Errors));
            }
            
            var user = await _userManager.FindByNameAsync(model.Username);
            await _userManager.AddPasswordAsync(user, model.Password);
            await _userManager.AddToRoleAsync(user, "Admin");

            await _signInManager.SignInWithClaimsAsync(user, true, new[] { new Claim("PasswordLogin", "true") });

            return RedirectToAction("Index", "Home");
        }
    }
}