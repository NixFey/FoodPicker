using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using FoodPicker.Web.Data;
using FoodPicker.Web.Enums;
using FoodPicker.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FoodPicker.Web.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class WeekController : Controller
    {
        private readonly ILogger<WeekController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public WeekController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<WeekController> logger, ApplicationDbContext db)
        {
            _userManager = userManager;
            _logger = logger;
            _db = db;
        }

        public class WeekListViewModel
        {
            public MealWeek MealWeek { get; set; }
            public bool CurrentUserVotingComplete { get; set; }
        }
        
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var mealWeeks = await _db.MealWeeks.ToListAsync();
            
            var viewModel = new List<WeekListViewModel>();
            
            foreach (var mealWeek in mealWeeks)
            {
                var numMealVotes = await _db.MealVotes.Where(x =>
                    x.Meal.MealWeekId == mealWeek.Id && x.UserId == _userManager.GetUserId(User) &&
                    x.VoteOption != null).CountAsync();

                var numMeals = await _db.Meals.CountAsync(x => x.MealWeekId == mealWeek.Id);
                
                viewModel.Add(new WeekListViewModel
                {
                    MealWeek = mealWeek,
                    CurrentUserVotingComplete = numMealVotes >= numMeals
                });
            }
            return View("List", viewModel);
        } 
        
        [HttpGet]
        [Route("Create")]
        [Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            MealWeek model;
            if (id is null or 0)
            {
                var latestMealWeek = await _db.MealWeeks.OrderByDescending(x => x.DeliveryDate).FirstOrDefaultAsync();
                model = new MealWeek
                {
                    DeliveryDate = latestMealWeek?.DeliveryDate.AddDays(7) ?? default
                };
            }
            else
            {
                model = await _db.MealWeeks.Include(x => x.Meals).FirstOrDefaultAsync(x => x.Id == id);
            }
            return View(model);
        } 
        
        [HttpPost]
        [Route("Create", Name = "WeekCreate")]
        [Route("{id:int}", Name = "WeekEdit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOrEdit(int? id, [FromForm] MealWeek model)
        {
            MealWeek dbModel;
            if (id is 0 or null)
            {
                dbModel = new MealWeek
                {
                    DeliveryDate = model.DeliveryDate,
                    MealWeekStatus = model.MealWeekStatus,
                };
                _db.MealWeeks.Add(dbModel);
            }
            else
            {
                dbModel = await _db.MealWeeks.FindAsync(id);
                dbModel.DeliveryDate = model.DeliveryDate;
                dbModel.MealWeekStatus = model.MealWeekStatus;
            }
            
            await _db.SaveChangesAsync();
            return RedirectToRoute("WeekEdit", new {id = dbModel.Id});
        }

        [HttpPost("[action]/{id:int}")]
        public async Task<IActionResult> GenerateMeals(int? id)
        {
            if (id is 0 or null) return BadRequest();
            var week = await _db.MealWeeks.Include(x => x.Meals).FirstOrDefaultAsync(x => x.Id == id);
            if (week == null) return BadRequest();
            
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(week.HelloFreshMenuUrl);
            var mealTitles = document.QuerySelectorAll("h4");

            var meals = new List<Meal>();
            
            foreach (var mealTitle in mealTitles)
            {
                var title = mealTitle.TextContent;
                var description = mealTitle.ParentElement?.ParentElement?.QuerySelector("div+span")?.TextContent;
                var imageUrl = mealTitle.ParentElement?.ParentElement?.ParentElement?.ParentElement?.QuerySelector("img")?
                    .GetAttribute("src");
                // <h4>.parentElement.parentElement.querySelectorAll('[role=button]>span')
                var majorTags = mealTitle.ParentElement?.ParentElement?.QuerySelector("[role='button']")?.TextContent;
                
                meals.Add(new Meal
                {
                    MealWeekId = week.Id,
                    Name = title,
                    Description = description,
                    ImageUrl = imageUrl,
                    Tags = majorTags
                });
            }

            if (meals.Any())
            {
                _db.Meals.RemoveRange(week.Meals);
                await _db.Meals.AddRangeAsync(meals);
                week.MealWeekStatus = MealWeekStatus.Active;
                await _db.SaveChangesAsync();   
            }

            return RedirectToAction("Index");
        }

        public class MealVoteViewModel
        {
            public MealWeek MealWeek { get; set; }
            public List<MealVote> UserMealVotes { get; set; }
            public ILookup<int, MealRating> PreviousRatings { get; set; }
        }
        [HttpGet("Vote/{id:int}")]
        public async Task<IActionResult> Vote(int? id)
        {
            var model = await _db.MealWeeks.Include(x => x.Meals).FirstOrDefaultAsync(x => x.Id == id);
            var weekVotes = await _db.MealVotes
                .Where(x => x.UserId == _userManager.GetUserId(User) && x.Meal.MealWeekId == id).ToListAsync();
            var previousRatings = _db.MealRatings.Include(x => x.Meal)
                .Where(x => model.Meals.Select(y => y.Name).Contains(x.Meal.Name)).AsEnumerable();
            var ratingLookup = previousRatings.ToLookup(x => model.Meals.Single(y => y.Name == x.Meal.Name).Id);
            
            return View(new MealVoteViewModel
            {
                MealWeek = model,
                UserMealVotes = model.Meals.Select(x => weekVotes.FirstOrDefault(y => y.MealId == x.Id) ?? new MealVote
                {
                    MealId = x.Id
                }).ToList(),
                PreviousRatings = ratingLookup
            });
        } 
        
        [HttpPost("Vote/{id:int}")]
        public async Task<IActionResult> Vote(int? id, [FromForm] MealVoteViewModel model)
        {
            foreach (var vote in model.UserMealVotes)
            {
                vote.UserId = _userManager.GetUserId(User);
                if (vote.Id == 0)
                {
                    _db.MealVotes.Add(vote);
                }
                else
                {
                    _db.Entry(vote).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public class ViewResultsViewModel
        {
            public MealWeek Week { get; set; }
            public List<ApplicationUser> ParticipatingUsers { get; set; }

            public List<MealResult> MealResults { get; set; } = new List<MealResult>();
            public Dictionary<int, bool> MealsSelected { get; set; } = new Dictionary<int, bool>();
            public bool Editable { get; set; }
        }

        public class MealResult
        {
            public Meal Meal { get; set; }
            public List<MealVote> Votes { get; set; }
            public double Score { get; set; }
        }
        [HttpGet("Results/{weekId:int}")]
        public async Task<IActionResult> ViewResults(int? weekId)
        {
            if (weekId is 0 or null) return BadRequest();
            var week = await _db.MealWeeks.Include(x => x.Meals).FirstOrDefaultAsync(x => x.Id == weekId);
            if (week == null) return BadRequest();

            var model = new ViewResultsViewModel
            {
                Week = week,
                ParticipatingUsers = await _userManager.Users.ToListAsync(),
                Editable = week.CanVote
            };
            var meals = week.Meals;
            var weekVotes = await _db.MealVotes.Where(x => x.Meal.MealWeekId == week.Id).ToListAsync();
            foreach (var meal in meals)
            {
                model.MealsSelected[meal.Id] = meal.SelectedForOrder ?? false;
                var mealVotes = weekVotes.Where(x => x.MealId == meal.Id).ToList();
                
                var score = mealVotes.Sum(vote => vote.VoteOption switch
                {
                    MealVoteOption.Yes => 1,
                    MealVoteOption.Maybe => 0.5,
                    MealVoteOption.No => 0,
                    null => 0,
                    _ => throw new Exception("Unknown vote option")
                });
                
                model.MealResults.Add(new MealResult
                {
                    Meal = meal,
                    Votes = mealVotes,
                    Score = score 
                });
            }
            return View(model);
        }

        [HttpPost("Results/{weekId:int}")]
        public async Task<IActionResult> ViewResults(int? weekId, [FromForm] ViewResultsViewModel model, string action)
        {
            foreach (var (key, value) in model.MealsSelected)
            {
                var meal = new Meal
                {
                    Id = key,
                    SelectedForOrder = value
                };
                _db.Meals.Attach(meal);
                _db.Entry(meal).Property(x => x.SelectedForOrder).IsModified = true;
            }

            if (action == "saveLock")
            {
                var week = await _db.MealWeeks.FindAsync(weekId);
                week.MealWeekStatus = MealWeekStatus.Past;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("ViewResults", new {weekId});
        }

        public class MealDetailsViewModel
        {
            public Meal Meal { get; set; }
            public List<MealRating> PreviousRatings { get; set; }
        }

        [HttpGet("MealDetailsModal/{mealId:int}")]
        public async Task<ActionResult> MealDetailsModal(int? mealId)
        {
            if (mealId is 0 or null) return BadRequest();
            var meal = await _db.Meals.FirstOrDefaultAsync(x => x.Id == mealId);
            if (meal == null) return BadRequest();
            
            var previousRatings = _db.MealRatings.Include(x => x.Meal)
                .Where(x => meal.Name == x.Meal.Name).ToList();

            return PartialView("_MealDetailsModal", new MealDetailsViewModel
            {
                Meal = meal,
                PreviousRatings = previousRatings 
            });
        }

        [HttpDelete("{weekId:int}")]
        [Authorize(AuthorizationPolicies.AccessInternalAdminAreas)]
        public async Task<ActionResult> Delete(int? weekId)
        {
            if (weekId is 0 or null) return BadRequest();

            var week = await _db.MealWeeks.FirstOrDefaultAsync(x => x.Id == weekId);
            if (week == null) return BadRequest();
            var meals = _db.Meals.Where(x => x.MealWeekId == week.Id).ToList();
            var mealIds = meals.Select(x => x.Id).ToList();
            
            var votes = _db.MealVotes.Where(x => mealIds.Contains(x.MealId)).AsEnumerable();
            _db.MealVotes.RemoveRange(votes);

            var ratings = _db.MealRatings.Where(x => mealIds.Contains(x.MealId)).AsEnumerable();
            _db.MealRatings.RemoveRange(ratings);
            
            _db.Meals.RemoveRange(meals);
            _db.MealWeeks.Remove(week);

            await _db.SaveChangesAsync();
            
            return Ok();
        }
    }
}