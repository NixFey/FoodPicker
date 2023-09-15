using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using Humanizer;
using Microsoft.Extensions.Configuration;
using TimeZoneConverter;

namespace FoodPicker.Infrastructure.Services
{
    public class HomeChefMealService : MealService
    {
        public override string MealServiceName => "Home Chef";

        private readonly PersistentConfigRepository _configRepo;
        
        public HomeChefMealService(IConfiguration configuration, PersistentConfigRepository configRepo) : base(configuration)
        {
            _configRepo = configRepo;
        }
        
        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public override string GetMenuUrlForMealWeek(MealWeek week)
        {
            var deliveryDate = week.DeliveryDate;
            var slugDate = StartOfWeek(deliveryDate.AddDays(-7), DayOfWeek.Monday);
            
            return "https://www.homechef.com/our-menus/" + slugDate.ToString("dd-MMM-yy").ToLower() + "/standard";
        }

        public override async Task<List<Meal>> GetMealsForMealWeek(MealWeek week)
        {
            var token = (await _configRepo.GetByCodeOrNull("HomeChefAccessToken")).Value;
            if (string.IsNullOrEmpty(token)) throw new ApplicationException("Access token not provided");
            
            var deliveryDate = week.DeliveryDate;
            var slugDate = StartOfWeek(deliveryDate, DayOfWeek.Monday);
            var menuUrl = "https://www.homechef.com/api/v2/menus/" + slugDate.ToString("dd-MMM-yyyy").ToLower();
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var mealResponse = await httpClient.GetFromJsonAsync<JsonDocument>(menuUrl);

            if (mealResponse == null) throw new ApplicationException("Unable to get meals from service provider");

            var mealOptions = new Dictionary<string, JsonElement>(mealResponse.RootElement.GetProperty("meal_option_groups")
                .EnumerateArray()
                .Select(x => new KeyValuePair<string, JsonElement>(x.GetProperty("meal_id").GetString(), x)));

            var meals = new List<Meal>();
            foreach (var mealElement in mealResponse.RootElement.GetProperty("meals").EnumerateArray())
            {
                if (new [] { "extras", "bundle-and-save", "lunch" }.Contains(mealElement.GetProperty("menu_category")
                        .GetString()))
                {
                    continue;
                }
                var mealId = mealElement.GetProperty("id").GetString() ?? "";
                var description = mealElement.GetProperty("subtitle").GetString();
                if (mealOptions.TryGetValue(mealId, out var option))
                {
                    description += $"\n{option.GetProperty("title").GetString()}";
                    foreach (var mealOption in option.GetProperty("meal_options").EnumerateArray())
                    {
                        description += $"\n - {mealOption.GetProperty("display_name").GetString()}";
                    }
                }

                var tags = new List<string> { mealElement.GetProperty("primary_label").GetString() };
                tags.AddRange(mealElement.GetProperty("tags").EnumerateArray().Select(t => t.GetString().Transform(To.TitleCase)));
    
                var meal = new Meal()
                {
                    MealWeekId = week.Id,
                    Name = mealElement.GetProperty("title").GetString(),
                    Description = description,
                    ImageUrl = mealElement.GetProperty("photo").GetString(),
                    Url = mealElement.GetProperty("url").GetString()?.Replace("cs.homechef.com", "homechef.com"),
                    MealConceptId = mealElement.GetProperty("meal_concept").GetString(),
                    Tags = string.Join(';', tags.Where(t => !string.IsNullOrEmpty(t))),
                };
                
                meals.Add(meal);
            }

            return meals;
        }

        public override DateTime GetUtcOrderDeadlineForDeliveryDate(DateTime deliveryDate)
        {
            var centralTime = TZConvert.GetTimeZoneInfo("America/Chicago");
            var subtractedDelivery = StartOfWeek(deliveryDate, DayOfWeek.Friday);
            
            var deadline = new DateTime
            (
                year: subtractedDelivery.Year,
                month: subtractedDelivery.Month,
                day: subtractedDelivery.Day,
                hour: 12,
                minute: 00,
                second: 00,
                kind: DateTimeKind.Unspecified
            );
            return TimeZoneInfo.ConvertTimeToUtc(deadline, centralTime);
        }
    }
}