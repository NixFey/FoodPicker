using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Data;
using FoodPicker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace FoodPicker.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class RatingController : Controller
    {
        private readonly ILogger<RatingController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public RatingController(UserManager<ApplicationUser> userManager, ILogger<RatingController> logger, ApplicationDbContext db)
        {
            _userManager = userManager;
            _logger = logger;
            _db = db;
        }

        public class ListViewModel
        {
            public List<Meal> MealsMissingRating { get; set; }
            public List<MealRating> MealRatings { get; set; }
        }
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = new ListViewModel
            {
                MealRatings = await _db.MealRatings.Include(x => x.Meal).ThenInclude(x => x.MealWeek).ToListAsync(),
                MealsMissingRating = await _db.Meals.Include(x => x.MealRating).Include(x => x.MealWeek)
                    .Where(x => x.SelectedForOrder == true && x.MealRating == null)
                    .ToListAsync()
            };
            return View("List", model);
        }
        
        [HttpGet]
        [Route("Create/{mealId}")]
        [Route("{id:int}")]
        public async Task<IActionResult> CreateOrEdit(int? id, int? mealId)
        {
            MealRating model;
            if (id is null or 0 && mealId is not null and not 0)
            {
                model = new MealRating()
                {
                    Meal = await _db.Meals.FindAsync(mealId),
                };
            }
            else
            {
                model = await _db.MealRatings.Include(x => x.Meal).FirstOrDefaultAsync(x => x.Id == id);
            }
            return View(model);
        } 
        
        [HttpPost]
        [Route("Create/{mealId}", Name = "RatingCreate")]
        [Route("{id:int}", Name = "RatingEdit")]
        public async Task<IActionResult> CreateOrEdit(int? id, int? mealId, [FromForm] MealRating model)
        {
            MealRating dbModel;
            if (id is 0 or null && mealId is not null and not 0)
            {
                dbModel = new MealRating
                {
                    MealId = (int) mealId,
                    Rating = model.Rating,
                    RatingComment = model.RatingComment,
                    RatingTime = DateTime.Now,
                };
                _db.MealRatings.Add(dbModel);
            }
            else
            {
                dbModel = await _db.MealRatings.FindAsync(id);
                dbModel.Rating = model.Rating;
                dbModel.RatingComment = model.RatingComment;
                dbModel.RatingTime = DateTime.Now;
            }
            
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}