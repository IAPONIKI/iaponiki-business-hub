using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Models.ViewComponents;
using WebApplication3.Services;

namespace WebApplication3.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PermissionService _permissionService;

        public NavigationViewComponent(
            UserManager<ApplicationUser> userManager,
            PermissionService permissionService)
        {
            _userManager = userManager;
            _permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new NavigationViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = user.Id;

                // Check permissions for different menu items
                model.CanViewAdminMenu = await _permissionService.UserHasPermissionAsync(userId, Permissions.ViewAdminMenu);
                model.CanViewHRMenu = await _permissionService.UserHasPermissionAsync(userId, Permissions.ViewHRMenu);
                model.CanViewFinanceMenu = await _permissionService.UserHasPermissionAsync(userId, Permissions.ViewFinanceMenu);
                model.CanViewMarketingMenu = await _permissionService.UserHasPermissionAsync(userId, Permissions.ViewMarketingMenu);

                // Get specific permissions for more granular control
                model.CanManageUsers = await _permissionService.UserHasPermissionAsync(userId, Permissions.ManageUsers);
                model.CanManageRoles = await _permissionService.UserHasPermissionAsync(userId, Permissions.ManageRoles);
                model.CanManagePermissions = await _permissionService.UserHasPermissionAsync(userId, Permissions.ManagePermissions);

                // Additional details
                model.UserName = user.FirstName + " " + user.LastName;
                model.UserEmail = user.Email;
                model.Department = user.Department;

                var roles = await _userManager.GetRolesAsync(user);
                model.IsAdmin = roles.Contains("Admin");
            }

            return View(model);
        }
    }
}