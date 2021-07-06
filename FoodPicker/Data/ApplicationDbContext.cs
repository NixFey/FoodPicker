using System;
using System.Collections.Generic;
using System.Text;
using FoodPicker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MealWeek> MealWeeks { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<MealVote> MealVotes { get; set; }
        public DbSet<MealRating> MealRatings { get; set; }
    }
}