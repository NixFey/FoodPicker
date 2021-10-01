using System.ComponentModel.DataAnnotations.Schema;

namespace FoodPicker.Infrastructure.Models
{
    public class MealVote : BaseEntity
    {
        public int MealId { get; set; }
        
        [ForeignKey(nameof(MealId))]
        public Meal Meal { get; set; }
        
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int VoteOptionId { get; set; }
        public VoteOption VoteOption { get; set; }
        public string Comment { get; set; }
    }
    
    public enum MealVoteOption
    {
        Yes = 1,
        Maybe = 2,
        No = 3
    }
}