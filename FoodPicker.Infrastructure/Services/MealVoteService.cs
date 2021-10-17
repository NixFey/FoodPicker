using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FoodPicker.Infrastructure.Services
{
    public class MealVoteService : IService
    {
        private readonly IMemoryCache _cache;
        private readonly EfRepository<VoteOption> _optionRepo;
        private readonly MealVoteRepository _voteRepo;
        private readonly EfRepository<Meal> _mealRepo;

        public MealVoteService(IMemoryCache cache, EfRepository<VoteOption> optionRepo, MealVoteRepository voteRepo, EfRepository<Meal> mealRepo)
        {
            _cache = cache;
            _optionRepo = optionRepo;
            _voteRepo = voteRepo;
            _mealRepo = mealRepo;
        }

        private async Task<double> VoteOptionToWeightAsync(VoteOption option)
        {
            if (option == null) return 0.0;
            return await _cache.GetOrCreateAsync($"VoteOption_{option.Id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return (await _optionRepo.GetByIdAsync(option.Id)).Weight;
            });
        }

        public async Task<IReadOnlyList<VoteOption>> GetAllVoteOptions()
        {
            return await _optionRepo.ListAllAsync();
        }

        public class VoteResult
        {
            public double Score { get; init; }
            public List<MealVote> Votes = new();
        }

        public async Task<Dictionary<int, VoteResult>> GetVoteResultsForWeekAsync(MealWeek week)
        {
            if (week.Meals == null) throw new ArgumentNullException(nameof(week.Meals));
            
            var weekVotes = await _voteRepo.GetAllVotesForWeekAsync(week);

            var resultDict = new Dictionary<int, VoteResult>();
            
            foreach (var meal in week.Meals)
            {
                var mealVotes = weekVotes.Where(x => x.MealId == meal.Id).ToList();
                
                var scoringTasks = mealVotes.Select(vote => VoteOptionToWeightAsync(vote.VoteOption))
                    .ToList();
                var score = 0.0;
                while (scoringTasks.Any())
                {
                    var finishedTask = await Task.WhenAny(scoringTasks);
                    scoringTasks.Remove(finishedTask);
                    score += await finishedTask;
                }

                resultDict[meal.Id] = new VoteResult
                {
                    Score = score,
                    Votes = mealVotes
                };
            }

            return resultDict;
        }

        public async Task<bool> IsVotingCompleteForUserForWeek(MealWeek week, string userId)
        {
            var numUserVotes = await _voteRepo.CountAsync(x =>
                x.Meal.MealWeekId == week.Id && x.UserId == userId &&
                x.VoteOption != null);
            
            var numMeals = await _mealRepo.CountAsync(x => x.MealWeekId == week.Id);

            return numUserVotes >= numMeals;
        }
    }
}