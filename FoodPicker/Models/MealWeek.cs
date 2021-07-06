using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FoodPicker.Models
{
    public class MealWeek
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Year {
            get
            {
                var cal = CultureInfo.CurrentCulture.Calendar;
                return cal.GetYear(DeliveryDate);
            }
        }
        [Required]
        public int WeekNo {
            get
            {
                var cal = CultureInfo.CurrentCulture.Calendar;
                return cal.GetWeekOfYear(DeliveryDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            }
        }

        public DateTime DeliveryDate { get; set; }

        public DateTime OrderDeadline
        {
            get
            {
                var subtractedDelivery = DeliveryDate.AddDays(-5);
                var pacificDeadline = new DateTime
                (
                    year: subtractedDelivery.Year,
                    month: subtractedDelivery.Month,
                    day: subtractedDelivery.Day,
                    hour: 23,
                    minute: 59,
                    second: 00
                );
                var deadline = TimeZoneInfo.ConvertTime(pacificDeadline,
                    TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"), TimeZoneInfo.Local);
                return deadline;
            }
        }

        public string HelloFreshMenuUrl => $"https://www.hellofresh.com/menus/{Year}-W{WeekNo}";

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