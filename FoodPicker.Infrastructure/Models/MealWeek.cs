using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FoodPicker.Infrastructure.Models
{
    public class MealWeek : BaseEntity
    {
        public DateTime DeliveryDate { get; set; }

        public DateTime OrderDeadline
        {
            get
            {
                var subtractedDelivery = DeliveryDate.AddDays(-5);
                var deadline = new DateTime
                (
                    year: subtractedDelivery.Year,
                    month: subtractedDelivery.Month,
                    day: subtractedDelivery.Day,
                    hour: 23,
                    minute: 59,
                    second: 00
                );
                return deadline;
            }
        }

        public List<Meal> Meals { get; set; }
        public MealWeekStatus MealWeekStatus { get; set; }

        public bool CanVote => MealWeekStatus == MealWeekStatus.Active;
    }

    public enum MealWeekStatus
    {
        Future = 1,
        Active = 2,
        Past = 3
    }
}