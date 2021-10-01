using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodPicker.Infrastructure.Models
{
    public class Meal : BaseEntity
    {
        public int MealWeekId { get; set; }
        
        [ForeignKey(nameof(MealWeekId))]
        public MealWeek MealWeek { get; set; }
        
        [Required]
        public string Name { get; set; }
        public string Tags { get; set; }
        [Required]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
        public bool? SelectedForOrder { get; set; }

        public List<MealVote> MealVotes { get; set; }
        
        public MealRating MealRating { get; set; }
    }
}