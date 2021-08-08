using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool VoteIsRequired { get; set; } = true;
    }
}