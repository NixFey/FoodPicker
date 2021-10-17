using System;
using System.Collections.Generic;

namespace FoodPicker.Infrastructure.Models
{
    public class MealWeek : BaseEntity
    {
        public DateTime DeliveryDate { get; set; }

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