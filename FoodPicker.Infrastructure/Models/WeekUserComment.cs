using System.ComponentModel.DataAnnotations;

namespace FoodPicker.Infrastructure.Models
{
    public class WeekUserComment : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int WeekId { get; set; }
        public MealWeek Week { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}