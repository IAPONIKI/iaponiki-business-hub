using Microsoft.AspNetCore.Identity;
using WebApplication3.Data;
using WebApplication3.Services;

namespace WebApplication3.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            PermissionService permissionService)
        {
            // Create roles if they don't exist
            string[] roles = { "Admin", "HR", "Finance", "Marketing", "Sales" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user if it doesn't exist
            var adminUser = await userManager.FindByNameAsync("admin@iaponiki.gr");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@iaponiki.gr",
                    Email = "admin@iaponiki.gr",
                    FirstName = "Admin",
                    LastName = "User",
                    Department = "Administration",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create sample department users
            await CreateSampleUserIfNotExists(userManager, "hr@iaponiki.gr", "HR123!", "HR");
            await CreateSampleUserIfNotExists(userManager, "finance@iaponiki.gr", "Finance123!", "Finance");
            await CreateSampleUserIfNotExists(userManager, "marketing@iaponiki.gr", "Marketing123!", "Marketing");
            await CreateSampleUserIfNotExists(userManager, "sales@iaponiki.gr", "Sales123!", "Sales");

            // Initialize default permissions for roles
            await permissionService.InitializeDefaultPermissionsAsync();
        }

        private static async Task CreateSampleUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByNameAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = role,
                    LastName = "User",
                    Department = role,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}