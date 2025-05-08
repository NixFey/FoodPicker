using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Infrastructure.Services;
using FoodPicker.Web.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPicker.Web.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AuthorizationPolicies.AllowApi)]
    public class ApiController : Controller
    {
        private readonly MealWeekRepository _mealWeekRepo;
        private readonly MealVoteRepository _mealVoteRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MealService _mealService;
        private readonly MealVoteService _mealVoteService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(MealWeekRepository mealWeekRepo, MealVoteRepository mealVoteRepo, UserManager<ApplicationUser> userManager, MealService mealService, ILogger<ApiController> logger, MealVoteService mealVoteService)
        {
            _mealWeekRepo = mealWeekRepo;
            _mealVoteRepo = mealVoteRepo;
            _userManager = userManager;
            _mealService = mealService;
            _logger = logger;
            _mealVoteService = mealVoteService;
        }

        public class NextWeekResult
        {
            public string WeekStatus { get; set; }
            public DateTime OrderDeadline { get; set; }
            public List<string> PendingVotes { get; set; }
            public List<string> FullyVoted { get; set; }
        }

        [HttpGet("[action]")]
        public async Task<NextWeekResult> NextWeek()
        {
            var week = await _mealWeekRepo.GetCurrentWithMeals();
            
            if (week == null) return null;

            var numMeals = week.Meals.Count;

            var mealVotes = _mealVoteRepo.GetVotesForWeekAsEnumerable(week);

            var fullyVotedUserIds = from v in mealVotes
                group v by v.UserId into u
                where u.Count() >= numMeals
                select u.Key;

            var users = _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.Name).ToList();

            var fullyVotedNames =
                users.Where(x => fullyVotedUserIds.Contains(x.Id)).Select(x => x.Name).ToList();

            var pendingVoteNames = users.Where(x => !fullyVotedUserIds.Contains(x.Id) && x.VoteIsRequired)
                .Select(x => x.Name).ToList();

            return new NextWeekResult
            {
                WeekStatus = week.MealWeekStatus.ToString(),
                OrderDeadline = _mealService.GetUtcOrderDeadlineForDeliveryDate(week.DeliveryDate),
                PendingVotes = pendingVoteNames,
                FullyVoted = fullyVotedNames
            };
        }

        /// <remarks>
        /// NOTE: This endpoint creates new weeks. In my home setup, this is behind a WAF and cannot be called except
        /// for trusted clients. If you're running this at home, beware.
        /// </remarks>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> TryCreateNewWeek([FromQuery] bool skip = false)
        {
            var latestMealWeek = await _mealWeekRepo.GetLatest();
            var week = new MealWeek
            {
                DeliveryDate = latestMealWeek?.DeliveryDate.AddDays(7) ?? DateTime.Today,
                MealWeekStatus = MealWeekStatus.Active
            };

            try
            {
                if (skip) await _mealService.SkipWeek(week);
                var mealsForNextDelivery = await _mealService.GetMealsForMealWeek(week);
                if (mealsForNextDelivery.Count == 0)
                    throw new ApplicationException("No meals reported from meal service");

                week.Meals = mealsForNextDelivery;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Unable to create next week: failed to fetch meals", e);
                return NoContent();
            }

            await _mealWeekRepo.AddAsync(week);
            await _mealVoteService.ProcessAutoVotes(week);

            return Ok();
        }
    }
}
