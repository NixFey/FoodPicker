using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using TimeZoneConverter;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace FoodPicker.Infrastructure.Services
{
    public class HelloFreshMealService : MealService
    {
        public override string MealServiceName => "Hello Fresh";

        private readonly PersistentConfigRepository _configRepo;

        public HelloFreshMealService(IConfiguration configuration, PersistentConfigRepository configRepo) : base(configuration)
        {
            _configRepo = configRepo;
        }

        private string GetWeekCodeForDate(DateTime date)
        {
            var cal = CultureInfo.CurrentCulture.Calendar;
            var year = cal.GetYear(date);
            var weekNo = cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);
            return $"{year}-W{weekNo}";
        }
        
        public override string GetMenuUrlForMealWeek(MealWeek week)
        {
            return $"https://www.hellofresh.com/menus/{GetWeekCodeForDate(week.DeliveryDate)}";
        }

        public override async Task<List<Meal>> GetMealsForMealWeek(MealWeek week)
        {
            var accessToken = await RefreshAuthentication();
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var mealResponse = await httpClient.GetFromJsonAsync<JsonDocument>(
                "https://www.hellofresh.com/gw/my-deliveries/menu?delivery-option=US-2-0800-2000&locale=en-US&postcode=48166&preference=quick&product-sku=US-CBT8-2-4-0&servings=4&subscription=17252914&week=" +
                GetWeekCodeForDate(week.DeliveryDate));

            var meals = new List<Meal>();
            foreach (var meal in mealResponse?.RootElement.GetProperty("meals").EnumerateArray())
            {
                string tags = null;
                if (meal.GetProperty("recipe").TryGetProperty("label", out var label))
                    tags = label.GetProperty("text").GetString();
                meals.Add(new Meal
                {
                    MealWeekId = week.Id,
                    Name = meal.GetProperty("recipe").GetProperty("name").GetString(),
                    Description = meal.GetProperty("recipe").GetProperty("headline").GetString(),
                    ImageUrl = meal.GetProperty("recipe").GetProperty("image").GetString(),
                    Tags = tags
                });
            }

            return meals;
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

        public async Task<string> RefreshAuthentication()
        {
            using var httpClient = new HttpClient();
            var refreshResult = await httpClient.PostAsync("https://www.hellofresh.com/gw/refresh",
                new StringContent("{\"refresh_token\":\"" +
                                  (await _configRepo.GetByCodeOrNull("HelloFreshRefreshToken")).Value + "\"}", Encoding.Default, MediaTypeNames.Application.Json));
            if (!refreshResult.IsSuccessStatusCode)
                throw new ApplicationException("Unable to refresh Hello Fresh auth");
            
            var refreshContent = JsonSerializer.Deserialize<JsonDocument>(await refreshResult.Content.ReadAsStringAsync());
            await _configRepo.UpdateByCode("HelloFreshRefreshToken", refreshContent?.RootElement.GetProperty("refresh_token").GetString());
            return refreshContent?.RootElement.GetProperty("access_token").GetString();

        }
    }
}