using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Web.Data;
using FoodPicker.Web.Enums;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Web.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class WeekController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MealService _mealService;
        private readonly MealWeekRepository _mealWeekRepo;
        private readonly MealVoteRepository _mealVoteRepo;
        private readonly MealRatingRepository _mealRatingRepo;
        private readonly EfRepository<Meal> _mealRepo;
        private readonly MealVoteService _mealVoteService;
        private readonly WeekUserCommentRepository _commentRepository;

        public WeekController(UserManager<ApplicationUser> userManager, MealService mealService, MealWeekRepository mealWeekRepo, EfRepository<Meal> mealRepo, MealVoteRepository mealVoteRepo, MealVoteService mealVoteService, MealRatingRepository mealRatingRepo, WeekUserCommentRepository commentRepository)
        {
            _userManager = userManager;
            _mealService = mealService;
            _mealWeekRepo = mealWeekRepo;
            _mealRepo = mealRepo;
            _mealVoteRepo = mealVoteRepo;
            _mealVoteService = mealVoteService;
            _mealRatingRepo = mealRatingRepo;
            _commentRepository = commentRepository;
        }

        public class WeekListViewModel
        {
            public MealWeek MealWeek { get; set; }
            public bool CurrentUserVotingComplete { get; set; }
        }
        
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var mealWeeks = await _mealWeekRepo.ListAllAsync();
            
            var viewModel = new List<WeekListViewModel>();
            foreach (var mealWeek in mealWeeks)
            {
                viewModel.Add(new WeekListViewModel
                {
                    MealWeek = mealWeek,
                    CurrentUserVotingComplete =
                        await _mealVoteService.IsVotingCompleteForUserForWeek(mealWeek, _userManager.GetUserId(User))
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

            var isCreating = id is null or 0;
            if (isCreating)
            {
                var latestMealWeek = await _mealWeekRepo.GetLatest();
                model = new MealWeek
                {
                    DeliveryDate = latestMealWeek?.DeliveryDate.AddDays(7) ?? DateTime.Today
                };
            }
            else
            {
                // Editing
                model = await _mealWeekRepo.GetByIdWithMealsAsync((int) id);

                if (model == null) throw new ApplicationException("Week not found");
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
                
                await _mealWeekRepo.AddAsync(dbModel);
            }
            else
            {
                dbModel = await _mealWeekRepo.GetByIdAsync((int) id);
                dbModel.DeliveryDate = model.DeliveryDate;
                dbModel.MealWeekStatus = model.MealWeekStatus;
                
                await _mealWeekRepo.UpdateAsync(dbModel);
            }

            return RedirectToRoute("WeekEdit", new {id = dbModel.Id});
        }

        [HttpPost("[action]/{id:int}")]
        public async Task<IActionResult> GenerateMeals(int? id)
        {
            if (id is 0 or null) return BadRequest();
            
            var week = await _mealWeekRepo.GetByIdWithMealsAsync((int) id);
            if (week == null) return BadRequest();

            var meals = await _mealService.GetMealsForMealWeek(week);

            if (meals.Any())
            {
                await _mealRepo.DeleteRangeAsync(week.Meals);
                await _mealRepo.AddRangeAsync(meals);
                week.MealWeekStatus = MealWeekStatus.Active;
                await _mealWeekRepo.UpdateAsync(week);   
            }

            return RedirectToRoute("WeekEdit", new {id});
        }

        public class MealVoteViewModel
        {
            public MealWeek MealWeek { get; set; }
            public List<MealVote> UserMealVotes { get; set; }
            public ILookup<int, MealRating> PreviousRatings { get; set; }
            [Required]
            [MaxLength(1000)]
            [Display(Name = "User Comment")]
            public string WeekUserComment { get; set; }
        }
        [HttpGet("Vote/{id:int}")]
        public async Task<IActionResult> Vote(int? id)
        {
            if (id is null or 0) return BadRequest();
            
            var model = await _mealWeekRepo.GetByIdWithMealsAsync((int) id);
            var weekVotes = await _mealVoteRepo.GetUserVotesForWeekAsync(model, _userManager.GetUserId(User));
            
            var ratingLookup = _mealRatingRepo.GetPreviousRatingsForMeals(model.Meals);
            
            return View(new MealVoteViewModel
            {
                MealWeek = model,
                UserMealVotes = model.Meals.Select(x => weekVotes.FirstOrDefault(y => y.MealId == x.Id) ?? new MealVote
                {
                    MealId = x.Id
                }).ToList(),
                PreviousRatings = ratingLookup,
                WeekUserComment = (await _commentRepository.GetCommentForWeekAndUser((int) id, _userManager.GetUserId(User)))?.Comment
            });
        }
        
        [HttpPost("Vote/{id:int}")]
        public async Task<IActionResult> Vote(int? id, [FromForm] MealVoteViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var dbComment = await _commentRepository.GetCommentForWeekAndUser((int) id, userId);
            if (dbComment == null)
            {
                dbComment = new WeekUserComment
                {
                    WeekId = (int)id,
                    UserId = userId,
                    Comment = model.WeekUserComment
                };
                await _commentRepository.AddAsync(dbComment);
            }
            else
            {
                dbComment.Comment = model.WeekUserComment;
                await _commentRepository.UpdateAsync(dbComment);
            }
            
            foreach (var dbVote in model.UserMealVotes.Select(vote => new MealVote
            {
                Id = vote.Id,
                MealId = vote.MealId,
                VoteOptionId = vote.VoteOptionId,
                Comment = vote.Comment,
                UserId = _userManager.GetUserId(User)
            }))
            {
                if (dbVote.Id == 0)
                {
                    await _mealVoteRepo.AddAsync(dbVote);
                }
                else
                {
                    await _mealVoteRepo.UpdateAsync(dbVote);
                }
            }

            return RedirectToAction("Index");
        }

        public class ViewResultsViewModel
        {
            public MealWeek Week { get; init; }
            public List<ApplicationUser> ParticipatingUsers { get; init; }

            public List<MealResult> MealResults { get; } = new();
            public Dictionary<int, bool> MealsSelected { get; } = new();
            public bool IsEditable { get; set; }
            public List<WeekUserComment> UserComments { get; set; }
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
            if (weekId is 0 or null) return NotFound();
            var week = await _mealWeekRepo.GetByIdWithMealsAsync((int) weekId);
            if (week == null) return NotFound();

            var model = new ViewResultsViewModel
            {
                Week = week,
                ParticipatingUsers = await _userManager.Users.ToListAsync(),
                IsEditable = week.CanVote,
                UserComments = await _commentRepository.GetCommentsForWeek((int) weekId)
            };
            var meals = week.Meals;
            
            var voteResults = await _mealVoteService.GetVoteResultsForWeekAsync(week);
            foreach (var meal in meals)
            {
                model.MealsSelected[meal.Id] = meal.SelectedForOrder ?? false;
                var voteResult = voteResults[meal.Id];
                
                model.MealResults.Add(new MealResult
                {
                    Meal = meal,
                    Votes = voteResult.Votes,
                    Score = voteResult.Score 
                });
            }
            return View(model);
        }

        [HttpPost("Results/{weekId:int}")]
        public async Task<IActionResult> ViewResults(int? weekId, [FromForm] ViewResultsViewModel model, string action)
        {
            if (weekId is 0 or null) return BadRequest();
            
            foreach (var (key, value) in model.MealsSelected)
            {
                var meal = new Meal
                {
                    Id = key,
                    SelectedForOrder = value
                };

                await _mealRepo.UpdateAsync(meal);
            }

            if (action == "saveLock")
            {
                var week = await _mealWeekRepo.GetByIdAsync((int) weekId);
                week.MealWeekStatus = MealWeekStatus.Past;
                await _mealWeekRepo.UpdateAsync(week);
            }
            
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
            var meal = await _mealRepo.GetByIdAsync((int) mealId);
            if (meal == null) return BadRequest();

            var previousRatings = _mealRatingRepo.GetPreviousRatingsForMeal(meal);

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

            var week = await _mealWeekRepo.GetByIdWithMealsAsync((int) weekId);
            if (week == null) return BadRequest();
            
            var meals = week.Meals;
            var mealIds = meals.Select(x => x.Id).ToList();

            var votes = await _mealVoteRepo.ListAsync(x => mealIds.Contains(x.MealId));
            await _mealVoteRepo.DeleteRangeAsync(votes);

            var ratings = await _mealRatingRepo.ListAsync(x => mealIds.Contains(x.MealId));
            await _mealRatingRepo.DeleteRangeAsync(ratings);
            
            await _mealRepo.DeleteRangeAsync(meals);
            await _mealWeekRepo.DeleteAsync(week);

            return Ok();
        }
    }
}