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
            if (mealService is HelloFreshMealService hfMealService)
            {
                _hfMealService = hfMealService;
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetIndex()
        {
            return View("Index", new PersistentConfigViewModel
            {
                HelloFreshRefreshToken = (await _configRepo.GetByCodeOrNull("HelloFreshRefreshToken"))?.Value,
                HomeChefAccessToken = (await _configRepo.GetByCodeOrNull("HomeChefAccessToken"))?.Value
            });
        }
        
        [HttpPost]
        public async Task<ActionResult> PostIndex([FromForm] PersistentConfigViewModel model)
        {
            await _configRepo.UpdateByCode("HelloFreshRefreshToken", model.HelloFreshRefreshToken);
            if (!string.IsNullOrEmpty(model.HelloFreshRefreshToken) && _hfMealService != null)
            {
                // Refresh the token before any browser has the chance to.
                await _hfMealService.RefreshAuthentication();   
            }

            await _configRepo.UpdateByCode("HomeChefAccessToken", model.HomeChefAccessToken);

            return RedirectToAction("GetIndex");
        }

        public class PersistentConfigViewModel
        {
            [Display(Name = "Hello Fresh Refresh Token")]
            public string HelloFreshRefreshToken { get; set; }
            
            [Display(Name = "Home Chef Access Token")]
            public string HomeChefAccessToken { get; set; }
        }
    }
}