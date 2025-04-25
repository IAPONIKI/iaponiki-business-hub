using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Services;

namespace WebApplication3.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PermissionService _permissionService;

        public PermissionTagHelper(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            PermissionService permissionService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _permissionService = permissionService;
        }

        [HtmlAttributeName("asp-permission")]
        public string Permission { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                output.SuppressOutput();
                return;
            }

            // Get the user ID
            var userId = _userManager.GetUserId(user);

            // Check if the user has the required permission
            if (!await _permissionService.UserHasPermissionAsync(userId, Permission))
            {
                output.SuppressOutput();
            }
        }
    }
}