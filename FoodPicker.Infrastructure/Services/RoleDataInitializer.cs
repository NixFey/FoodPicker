using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FoodPicker.Infrastructure.Services
{
    public static class RoleDataInitializer
    {
        public static async Task SeedData(RoleManager<IdentityRole> roleManager)
        {
            await CreateRoleIfNotExists(roleManager, "Admin");
        }

        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string name)
        {
            if (!(await roleManager.RoleExistsAsync(name)))
            {
                var role = new IdentityRole
                {
                    Name = name
                };
                await roleManager.CreateAsync(role);
            }
        }
    }
}