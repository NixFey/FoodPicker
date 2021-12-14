namespace FoodPicker.Infrastructure.Models
{
    public class PersistentConfig : BaseEntity
    {
        public string ConfigCode { get; set; }
        public string Value { get; set; }
    }
}