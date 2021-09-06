using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Infrastructure.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool VoteIsRequired { get; set; } = true;
    }
}