using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class MealVoteRepository : EfRepository<MealVote>
    {
        private readonly ApplicationDbContext _db;

        public MealVoteRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
        }

        public IEnumerable<MealVote> GetVotesForWeekAsEnumerable(MealWeek week)
        {
            return _db.MealVotes.Where(x => x.Meal.MealWeekId == week.Id && x.VoteOption != null).AsEnumerable();
        }

        public async Task<List<MealVote>> GetUserVotesForWeekAsync(MealWeek week, string userId, CancellationToken cancellationToken = default)
        {
            return await _db.MealVotes.Where(x => x.UserId == userId && x.Meal.MealWeekId == week.Id)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<List<MealVote>> GetAllVotesForWeekAsync(MealWeek week, CancellationToken cancellationToken = default)
        {
            return await _db.MealVotes.Include(x => x.VoteOption)
                .Where(x => x.Meal.MealWeekId == week.Id && x.VoteOption != null)
                .ToListAsync(cancellationToken);
        }
    }
}