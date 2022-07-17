using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodPicker.Web.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class ApiController : Controller
    {
        private readonly MealWeekRepository _mealWeekRepo;
        private readonly MealVoteRepository _mealVoteRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MealService _mealService;

        public ApiController(MealWeekRepository mealWeekRepo, MealVoteRepository mealVoteRepo, UserManager<ApplicationUser> userManager, MealService mealService)
        {
            _mealWeekRepo = mealWeekRepo;
            _mealVoteRepo = mealVoteRepo;
            _userManager = userManager;
            _mealService = mealService;
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
                where u.Count() == numMeals
                select u.Key;
            
            var users = _userManager.Users.ToList();

            var fullyVotedNames =
                users.Where(x => fullyVotedUserIds.Contains(x.Id)).Select(x => x.Name).ToList();

            var pendingVoteNames = users.Where(x => !fullyVotedUserIds.Contains(x.Id) && x.VoteIsRequired)
                .Select(x => x.Name).ToList();

            return new NextWeekResult
            {
                WeekStatus = week.MealWeekStatus.ToString(),
                OrderDeadline = _mealService.GetLocalOrderDeadlineForDeliveryDate(week.DeliveryDate),
                PendingVotes = pendingVoteNames,
                FullyVoted = fullyVotedNames
            };
        }
    }
}