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
        private readonly MealWeekRepository _weekRepo;
        private readonly EfRepository<Meal> _mealRepo;
        private readonly AutoVoteRepository _autoVoteRepo;

        public MealVoteService(IMemoryCache cache, EfRepository<VoteOption> optionRepo, MealVoteRepository voteRepo, EfRepository<Meal> mealRepo, MealWeekRepository weekRepo, AutoVoteRepository autoVoteRepo)
        {
            _cache = cache;
            _optionRepo = optionRepo;
            _voteRepo = voteRepo;
            _mealRepo = mealRepo;
            _weekRepo = weekRepo;
            _autoVoteRepo = autoVoteRepo;
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

        public async Task ProcessAutoVotes(MealWeek weekToUse = null, string userId = null)
        {
            // TODO: This whole algorithm can be improved. It was a last-minute addition to be fixed later
            var weeks = new List<MealWeek>();
            if (weekToUse is not null)
            {
                weeks.Add(weekToUse);
            }
            else
            {
                weeks.AddRange(await _weekRepo.ListAsync(x => x.MealWeekStatus == MealWeekStatus.Active));
            }

            IEnumerable<AutoVote> autoVotes;
            if (userId is not null)
            {
                autoVotes = await _autoVoteRepo.ListAsync(x => x.UserId == userId);
            }
            else
            {
                autoVotes = await _autoVoteRepo.ListAllAsync();
            }

            foreach (var week in weeks)
            {
                var votes = await _voteRepo.GetAllVotesForWeekAsync(week);
                var meals = (await _weekRepo.GetByIdWithMealsAsync(week.Id)).Meals;

                foreach (var meal in meals)
                {
                    foreach (var autoVote in autoVotes)
                    {
                        if (votes.Any(x => x.MealId == meal.Id && x.UserId == autoVote.UserId)) continue;
                        if (meal.Description.Contains(autoVote.Keyword, StringComparison.CurrentCultureIgnoreCase) || meal.Name.Contains(autoVote.Keyword, StringComparison.CurrentCultureIgnoreCase))
                        {
                            await _voteRepo.AddAsync(new MealVote()
                            {
                                MealId = meal.Id,
                                UserId = autoVote.UserId,
                                VoteOptionId = autoVote.VoteOptionId
                            });
                            // Only care about the first match
                            break;
                        }
                    }
                }
            }
        }
    }
}