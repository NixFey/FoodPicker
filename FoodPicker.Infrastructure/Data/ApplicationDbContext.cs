using FoodPicker.Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MealWeek> MealWeeks { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<MealVote> MealVotes { get; set; }
        public DbSet<MealRating> MealRatings { get; set; }
        public DbSet<VoteOption> VoteOptions { get; set; }
        public DbSet<WeekUserComment> WeekUserComments { get; set; }
        public DbSet<PersistentConfig> PersistentConfigs { get; set; }
    }
}