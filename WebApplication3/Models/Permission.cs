using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        // Flag to indicate if this permission is related to UI access/view
        public bool IsViewPermission { get; set; }
    }

    // Define permission constants for your application
    public static class Permissions
    {
        // HR Module
        public const string ViewHRMenu = "ViewHRMenu";
        public const string ManageJobs = "ManageJobs";
        public const string EditJobs = "EditJobs";
        public const string DeleteJobs = "DeleteJobs";
        public const string ExportHRData = "ExportHRData";

        // Finance Module
        public const string ViewFinanceMenu = "ViewFinanceMenu";
        public const string ViewInvoices = "ViewInvoices";
        public const string CompareInvoices = "CompareInvoices";
        public const string ManagePendingPayments = "ManagePendingPayments";

        // Marketing Module
        public const string ViewMarketingMenu = "ViewMarketingMenu";
        public const string ManageCarousels = "ManageCarousels";
        public const string ExportMarketingData = "ExportMarketingData";

        // Admin Module
        public const string ViewAdminMenu = "ViewAdminMenu";
        public const string ManageUsers = "ManageUsers";
        public const string ManageRoles = "ManageRoles";
        public const string ManagePermissions = "ManagePermissions";

        // Comprehensive list of all permissions
        public static List<Permission> AllPermissions = new List<Permission>
        {
            // HR Permissions
            new Permission { Name = ViewHRMenu, DisplayName = "View HR Menu", Description = "Can view the HR menu in navigation", Category = "HR", IsViewPermission = true },
            new Permission { Name = ManageJobs, DisplayName = "Manage Jobs", Description = "Can view and manage job postings", Category = "HR" },
            new Permission { Name = EditJobs, DisplayName = "Edit Jobs", Description = "Can edit job postings", Category = "HR" },
            new Permission { Name = DeleteJobs, DisplayName = "Delete Jobs", Description = "Can delete job postings", Category = "HR" },
            new Permission { Name = ExportHRData, DisplayName = "Export HR Data", Description = "Can export HR data to various formats", Category = "HR" },
            
            // Finance Permissions
            new Permission { Name = ViewFinanceMenu, DisplayName = "View Finance Menu", Description = "Can view the Finance menu in navigation", Category = "Finance", IsViewPermission = true },
            new Permission { Name = ViewInvoices, DisplayName = "View Invoices", Description = "Can view invoice data", Category = "Finance" },
            new Permission { Name = CompareInvoices, DisplayName = "Compare Invoices", Description = "Can upload and compare invoices", Category = "Finance" },
            new Permission { Name = ManagePendingPayments, DisplayName = "Manage Pending Payments", Description = "Can manage pending payments", Category = "Finance" },
            
            // Marketing Permissions
            new Permission { Name = ViewMarketingMenu, DisplayName = "View Marketing Menu", Description = "Can view the Marketing menu in navigation", Category = "Marketing", IsViewPermission = true },
            new Permission { Name = ManageCarousels, DisplayName = "Manage Carousels", Description = "Can manage website carousels", Category = "Marketing" },
            new Permission { Name = ExportMarketingData, DisplayName = "Export Marketing Data", Description = "Can export marketing data", Category = "Marketing" },
            
            // Admin Permissions
            new Permission { Name = ViewAdminMenu, DisplayName = "View Admin Menu", Description = "Can view the Admin menu in navigation", Category = "Admin", IsViewPermission = true },
            new Permission { Name = ManageUsers, DisplayName = "Manage Users", Description = "Can manage system users", Category = "Admin" },
            new Permission { Name = ManageRoles, DisplayName = "Manage Roles", Description = "Can manage roles and role assignments", Category = "Admin" },
            new Permission { Name = ManagePermissions, DisplayName = "Manage Permissions", Description = "Can manage permissions for roles", Category = "Admin" },
        };
    }
}