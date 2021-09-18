using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using FoodPicker.Infrastructure.Models;

namespace FoodPicker.Infrastructure.Services
{
    public class HelloFreshMealService : IMealService
    {
        public new readonly string MealServiceName = "Hello Fresh";
        
        public override string GetMenuUrlForMealWeek(MealWeek week)
        {
            var cal = CultureInfo.CurrentCulture.Calendar;
            var year = cal.GetYear(week.DeliveryDate);
            var weekNo = cal.GetWeekOfYear(week.DeliveryDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);
            return $"https://www.hellofresh.com/menus/{year}-W{weekNo}";
        }
        
        public override async Task<List<Meal>> GetMealsForMealWeek(MealWeek week)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(this.GetMenuUrlForMealWeek(week));
            var mealTitles = document.QuerySelectorAll("h4");

            return (from mealTitle in mealTitles
                let title = mealTitle.TextContent
                let description = mealTitle.ParentElement?.ParentElement?.QuerySelector("div+span")?.TextContent
                let imageUrl = mealTitle.ParentElement?.ParentElement?.ParentElement?.ParentElement?.QuerySelector("img")?.GetAttribute("src")
                let majorTags = mealTitle.ParentElement?.ParentElement?.QuerySelector("[role='button']")?.TextContent
                select new Meal
                {
                    MealWeekId = week.Id,
                    Name = title,
                    Description = description,
                    ImageUrl = imageUrl,
                    Tags = majorTags
                }).ToList();
        }
    }
}