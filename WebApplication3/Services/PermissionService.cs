using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Check if a user has a specific permission
        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Admin users have all permissions
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
                return true;

            // Check if any of the user's roles have the permission
            var roleIds = (await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync());

            return await _context.RolePermissions
                .AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.PermissionName == permission);
        }

        // Get all permissions for a role
        public async Task<List<string>> GetRolePermissionsAsync(string roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionName)
                .ToListAsync();
        }

        // Update permissions for a role
        public async Task UpdateRolePermissionsAsync(string roleId, List<string> permissions)
        {
            // Get current permissions
            var currentPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            // Remove permissions that are no longer assigned
            var permissionsToRemove = currentPermissions
                .Where(rp => !permissions.Contains(rp.PermissionName))
                .ToList();

            foreach (var permission in permissionsToRemove)
            {
                _context.RolePermissions.Remove(permission);
            }

            // Add new permissions
            var existingPermissionNames = currentPermissions.Select(rp => rp.PermissionName);
            var permissionsToAdd = permissions
                .Where(p => !existingPermissionNames.Contains(p))
                .Select(p => new RolePermission { RoleId = roleId, PermissionName = p })
                .ToList();

            await _context.RolePermissions.AddRangeAsync(permissionsToAdd);
            await _context.SaveChangesAsync();
        }

        // Initialize default permissions for built-in roles
        public async Task InitializeDefaultPermissionsAsync()
        {
            // Define default permissions for each built-in role
            var rolePermissions = new Dictionary<string, List<string>>()
            {
                { "Admin", Permissions.AllPermissions.Select(p => p.Name).ToList() },
                {
                    "HR",
                    new List<string>
                    {
                        Permissions.ViewHRMenu,
                        Permissions.ManageJobs,
                        Permissions.EditJobs,
                        Permissions.DeleteJobs,
                        Permissions.ExportHRData
                    }
                },
                {
                    "Finance",
                    new List<string>
                    {
                        Permissions.ViewFinanceMenu,
                        Permissions.ViewInvoices,
                        Permissions.CompareInvoices,
                        Permissions.ManagePendingPayments
                    }
                },
                {
                    "Marketing",
                    new List<string>
                    {
                        Permissions.ViewMarketingMenu,
                        Permissions.ManageCarousels,
                        Permissions.ExportMarketingData
                    }
                }
            };

            foreach (var rolePair in rolePermissions)
            {
                var role = await _roleManager.FindByNameAsync(rolePair.Key);
                if (role != null)
                {
                    await UpdateRolePermissionsAsync(role.Id, rolePair.Value);
                }
            }
        }
    }
}