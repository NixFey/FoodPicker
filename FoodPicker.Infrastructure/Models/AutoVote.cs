namespace FoodPicker.Infrastructure.Models
{
    public class AutoVote : BaseEntity
    {
        public string Keyword { get; set; }
        
        public int VoteOptionId { get; set; }
        
        public VoteOption VoteOption { get; set; }
        
        public string UserId { get; set; }
    }
}