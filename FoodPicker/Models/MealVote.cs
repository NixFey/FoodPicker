using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Models
{
    public class MealVote
    {
        public int Id { get; set; }
        public int MealId { get; set; }
        
        [ForeignKey(nameof(MealId))]
        public Meal Meal { get; set; }
        
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public MealVoteOption? VoteOption { get; set; }
        public string Comment { get; set; }
    }
    
    public enum MealVoteOption
    {
        Yes = 1,
        Maybe = 2,
        No = 3
    }
}