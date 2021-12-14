using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Services;
using FoodPicker.Web.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodPicker.Web.Controllers
{
    [Route("config")]
    [Authorize(Policy = AuthorizationPolicies.AccessInternalAdminAreas)]
    public class PersistentConfigController : Controller
    {
        private readonly PersistentConfigRepository _configRepo;
        private readonly HelloFreshMealService _hfMealService;

        public PersistentConfigController(PersistentConfigRepository configRepo, MealService mealService)
        {
            _configRepo = configRepo;
            _hfMealService = (HelloFreshMealService) mealService;
        }

        [HttpGet]
        public async Task<ActionResult> GetIndex()
        {
            return View("Index", new PersistentConfigViewModel
            {
                HelloFreshRefreshToken = (await _configRepo.GetByCodeOrNull("HelloFreshRefreshToken")).Value
            });
        }
        
        [HttpPost]
        public async Task<ActionResult> PostIndex([FromForm] PersistentConfigViewModel model)
        {
            await _configRepo.UpdateByCode("HelloFreshRefreshToken", model.HelloFreshRefreshToken);
            // Refresh the token before any browser has the chance to.
            await _hfMealService.RefreshAuthentication();
            return RedirectToAction("GetIndex");
        }

        public class PersistentConfigViewModel
        {
            [Required]
            [Display(Name = "Hello Fresh Refresh Token")]
            public string HelloFreshRefreshToken { get; set; }
        }
    }
}