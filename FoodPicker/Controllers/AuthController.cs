using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPicker.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AuthController> logger)
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
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            await _signInManager.SignInAsync(user, new AuthenticationProperties
            {
                IsPersistent = true
            });

            return RedirectToAction("Index", "Home");
        }
        
        public class RegisterViewModel
        {
            [Required]
            public string Username { get; set; }
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            var createResult = await _userManager.CreateAsync(new IdentityUser
            {
                UserName = model.Username
            });
            if (!createResult.Succeeded)
            {
                _logger.LogError("Error creating user: {Error}", string.Join(", ", createResult.Errors));
            }
            var user = await _userManager.FindByNameAsync(model.Username);
            await _signInManager.SignInAsync(user, new AuthenticationProperties
            {
                IsPersistent = true
            });

            return RedirectToAction("Index", "Home");
        }
    }
}