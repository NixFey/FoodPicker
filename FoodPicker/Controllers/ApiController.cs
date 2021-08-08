using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Data;
using FoodPicker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class ApiController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ApiController(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }

        public class NextWeekResult
        {
            public DateTime OrderDeadline { get; set; }
            public List<string> PendingVotes { get; set; }
            public List<string> FullyVoted { get; set; }
        }

        [HttpGet("[action]")]
        public async Task<NextWeekResult> NextWeek()
        {
            // TODO Clean me up
            var week = (await _db.MealWeeks.OrderByDescending(x => x.DeliveryDate).Include(x => x.Meals).ToListAsync())
                .FirstOrDefault(x =>
                    x.MealWeekStatus == MealWeekStatus.Active && x.OrderDeadline > DateTime.Now);

            if (week == null) return null;

            var numMeals = week.Meals.Count;

            var mealVotes = _db.MealVotes.Where(x => x.Meal.MealWeekId == week.Id && x.VoteOption != null).AsEnumerable();

            var fullyVotedUserIds = from v in mealVotes
                group v by v.UserId into u
                where u.Count() == numMeals
                select u.Key;
            
            var users = _db.Users.ToList();

            var fullyVotedUsernames =
                users.Where(x => fullyVotedUserIds.Contains(x.Id)).Select(x => x.UserName).ToList();
            
            var pendingVoteUserNames = users.Where(x => !fullyVotedUserIds.Contains(x.Id)).Select(x => x.UserName).ToList();

            return new NextWeekResult
            {
                OrderDeadline = week.OrderDeadline,
                PendingVotes = pendingVoteUserNames,
                FullyVoted = fullyVotedUsernames
            };
        }
    }
}