using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodPicker.Web.Models
{
    public class MealRating
    {
        public const int MaxRating = 4;
        public int Id { get; set; }
        public int MealId { get; set; }
        [ForeignKey(nameof(MealId))]
        public Meal Meal { get; set; }
        [Range(1,MaxRating)]
        [Display(Name = "Rating")]
        public int Rating { get; set; }
        [Display(Name = "Comment")]
        public string RatingComment { get; set; }
        public DateTime RatingTime { get; set; }
    }
}