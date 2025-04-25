namespace WebApplication3.Models.ViewComponents
{
    public class NavigationViewModel
    {
        // Permission-based flags
        public bool CanViewAdminMenu { get; set; } = false;
        public bool CanViewHRMenu { get; set; } = false;
        public bool CanViewFinanceMenu { get; set; } = false;
        public bool CanViewMarketingMenu { get; set; } = false;

        public bool CanManageUsers { get; set; } = false;
        public bool CanManageRoles { get; set; } = false;
        public bool CanManagePermissions { get; set; } = false;

        // Legacy role-based flags (can be removed once permissions fully implemented)
        public bool IsAdmin { get; set; } = false;

        // User info
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}