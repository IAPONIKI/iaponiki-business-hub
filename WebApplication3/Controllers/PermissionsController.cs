using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PermissionService _permissionService;

        public PermissionsController(
            RoleManager<IdentityRole> roleManager,
            PermissionService permissionService)
        {
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        // List all roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // Edit permissions for a role
        public async Task<IActionResult> EditRolePermissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Get current permissions for the role
            var currentPermissions = await _permissionService.GetRolePermissionsAsync(id);

            // Create view model with all available permissions
            var model = new RolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permissions = Permissions.AllPermissions.Select(p => new RolePermissionsViewModel.PermissionCheckbox
                {
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Category = p.Category,
                    IsAssigned = currentPermissions.Contains(p.Name),
                    IsViewPermission = p.IsViewPermission
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRolePermissions(RolePermissionsViewModel model, List<string> selectedPermissions)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                return NotFound();
            }

            // Only allow editing permissions for non-Admin roles
            if (role.Name == "Admin")
            {
                // Admin role should have all permissions
                selectedPermissions = Permissions.AllPermissions.Select(p => p.Name).ToList();
            }

            // Update the permissions
            await _permissionService.UpdateRolePermissionsAsync(model.RoleId, selectedPermissions ?? new List<string>());

            TempData["SuccessMessage"] = $"Permissions for role '{role.Name}' have been updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}