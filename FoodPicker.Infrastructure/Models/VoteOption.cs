namespace FoodPicker.Infrastructure.Models
{
    public class VoteOption : BaseEntity
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
    }
}