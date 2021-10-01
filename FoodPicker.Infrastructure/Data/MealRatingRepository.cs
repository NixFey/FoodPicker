using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class MealRatingRepository : EfRepository<MealRating>
    {
        private readonly ApplicationDbContext _db;

        public MealRatingRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
        }
        
        public async Task<List<MealRating>> ListAllWithWeekAndMealsAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.MealRatings.Include(x => x.Meal).ThenInclude(x => x.MealWeek)
                .ToListAsync(cancellationToken);
        }

        public async Task<MealRating> GetByIdWithMealAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.MealRatings.Include(x => x.Meal).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Meal>> GetMealsMissingRating(CancellationToken cancellationToken = default)
        {
            return await _db.Meals.Include(x => x.MealRating).Include(x => x.MealWeek)
                .Where(x => x.SelectedForOrder == true && x.MealRating == null)
                .ToListAsync(cancellationToken);
        }

        public ILookup<int, MealRating> GetPreviousRatingsForMeals(List<Meal> meals)
        {
            var ratings = _db.MealRatings.Include(x => x.Meal)
                .Where(x => meals.Select(y => y.Name).Contains(x.Meal.Name)).AsEnumerable();
            
            return ratings.ToLookup(x => meals.Single(y => y.Name == x.Meal.Name).Id);
        }

        public List<MealRating> GetPreviousRatingsForMeal(Meal meal)
        {
            return _db.MealRatings.Include(x => x.Meal)
                .Where(x => meal.Name == x.Meal.Name).ToList();
        }

        public async Task<List<MealVote>> GetUserVotesForWeekAsync(MealWeek week, string userId, CancellationToken cancellationToken = default)
        {
            return await _db.MealVotes.Where(x => x.UserId == userId && x.Meal.MealWeekId == week.Id && x.VoteOption != null)
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