using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;

namespace FoodPicker.Infrastructure.Services
{
    public abstract class IMealService
    {
        public readonly string MealServiceName = "Generic Meal Service";
        public abstract string GetMenuUrlForMealWeek(MealWeek week);
        public abstract Task<List<Meal>> GetMealsForMealWeek(MealWeek week);
    }
}