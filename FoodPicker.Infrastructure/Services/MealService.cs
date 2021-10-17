using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using TimeZoneConverter;

namespace FoodPicker.Infrastructure.Services
{
    public abstract class MealService
    {
        public virtual string MealServiceName => "Generic Meal Service";

        public abstract string GetMenuUrlForMealWeek(MealWeek week);
        public abstract Task<List<Meal>> GetMealsForMealWeek(MealWeek week);
        protected abstract DateTime GetUtcOrderDeadlineForDeliveryDate(DateTime deliveryDate);

        private readonly TimeZoneInfo _localTz;
        protected MealService(IConfiguration configuration)
        {
            _localTz = !string.IsNullOrEmpty(configuration["TimeZone"])
                ? TZConvert.GetTimeZoneInfo(configuration["TimeZone"])
                : TimeZoneInfo.Utc;
        }
        
        public DateTime GetLocalOrderDeadlineForDeliveryDate(DateTime deliveryDate)
        {
            var utcDeadline = GetUtcOrderDeadlineForDeliveryDate(deliveryDate);
            var localDeadline = TimeZoneInfo.ConvertTimeFromUtc(utcDeadline, _localTz);
            return localDeadline;
        }
    }
}