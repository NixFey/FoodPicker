using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Infrastructure.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool VoteIsRequired { get; set; } = true;
    }
}