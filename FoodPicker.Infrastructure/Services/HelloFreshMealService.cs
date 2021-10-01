using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using FoodPicker.Infrastructure.Models;
using TimeZoneConverter;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace FoodPicker.Infrastructure.Services
{
    public class HelloFreshMealService : MealService
    {
        public override string MealServiceName => "Hello Fresh";

        public HelloFreshMealService(IConfiguration configuration) : base(configuration)
        {
        }

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

        protected override DateTime GetUtcOrderDeadlineForDeliveryDate(DateTime deliveryDate)
        {
            // Hello fresh orders are due at 11:59 PM Pacific Time 5 days before the delivery
            var subtractedDelivery = deliveryDate.AddDays(-5);

            var pacificTime = TZConvert.GetTimeZoneInfo("America/Los_Angeles");

            var deadline = new DateTime
            (
                year: subtractedDelivery.Year,
                month: subtractedDelivery.Month,
                day: subtractedDelivery.Day,
                hour: 23,
                minute: 59,
                second: 00,
                kind: DateTimeKind.Unspecified
            );
            return TimeZoneInfo.ConvertTimeToUtc(deadline, pacificTime);
        }
    }
}