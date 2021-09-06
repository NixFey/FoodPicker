using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool VoteIsRequired { get; set; } = true;
    }
}