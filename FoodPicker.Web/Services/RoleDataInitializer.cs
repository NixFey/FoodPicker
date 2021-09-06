using System.Threading.Tasks;
using FoodPicker.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Web.Services
{
    public class RoleDataInitializer
    {
        public static async Task SeedData(RoleManager<IdentityRole> roleManager)
        {
            if (!(await roleManager.RoleExistsAsync("Admin")))
            {
                var role = new IdentityRole
                {
                    Name = "Admin"
                };
                await roleManager.CreateAsync(role);
            }
        }
    }
}