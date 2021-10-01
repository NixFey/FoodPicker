using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Web.Data;
using FoodPicker.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace FoodPicker.Web.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class RatingController : Controller
    {
        private readonly ILogger<RatingController> _logger;
        private readonly MealRatingRepository _mealRatingRepo;
        private readonly EfRepository<Meal> _mealRepo;

        public RatingController(ILogger<RatingController> logger, MealRatingRepository mealRatingRepo, EfRepository<Meal> mealRepo)
        {
            _logger = logger;
            _mealRatingRepo = mealRatingRepo;
            _mealRepo = mealRepo;
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
                MealRatings = await _mealRatingRepo.ListAllWithWeekAndMealsAsync(),
                MealsMissingRating = await _mealRatingRepo.GetMealsMissingRating()
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
                    Meal = await _mealRepo.GetByIdAsync((int) mealId),
                };
            }
            else
            {
                if (id != null)
                {
                    model = await _mealRatingRepo.GetByIdWithMealAsync((int)id);
                }
                else
                {
                    throw new ApplicationException("This shouldn't happen.");
                }
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
                await _mealRatingRepo.AddAsync(dbModel);
                return RedirectToAction("Index");
            }
            
            if (id != null)
            {
                dbModel = await _mealRatingRepo.GetByIdAsync((int)id);
            }
            else
            {
                throw new ApplicationException("This shouldn't happen.");
            }
            dbModel.Rating = model.Rating;
            dbModel.RatingComment = model.RatingComment;
            dbModel.RatingTime = DateTime.Now;

            await _mealRatingRepo.UpdateAsync(dbModel);
            return RedirectToAction("Index");
        }
    }
}