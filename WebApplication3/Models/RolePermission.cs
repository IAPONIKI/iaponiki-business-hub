using Microsoft.AspNetCore.Identity;

namespace WebApplication3.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        public string RoleId { get; set; }

        public string PermissionName { get; set; }

        // Navigation property (if using EF Core)
        public virtual IdentityRole Role { get; set; }
    }

    // View model for managing role permissions
    public class RolePermissionsViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionCheckbox> Permissions { get; set; } = new List<PermissionCheckbox>();

        public class PermissionCheckbox
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public bool IsAssigned { get; set; }
            public bool IsViewPermission { get; set; }
        }
    }
}