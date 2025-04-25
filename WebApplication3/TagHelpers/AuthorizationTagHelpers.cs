using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using WebApplication3.Data;

namespace WebApplication3.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-authorize")]
    [HtmlTargetElement(Attributes = "asp-authorize,asp-roles")]
    public class AuthorizeTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizeTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("asp-roles")]
        public string Roles { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                output.SuppressOutput();
                return;
            }

            if (!string.IsNullOrWhiteSpace(Roles))
            {
                var rolesSplit = Roles.Split(',').Select(r => r.Trim());
                var authorized = false;

                foreach (var role in rolesSplit)
                {
                    if (user.IsInRole(role))
                    {
                        authorized = true;
                        break;
                    }
                }

                if (!authorized)
                {
                    output.SuppressOutput();
                    return;
                }
            }
        }
    }

    [HtmlTargetElement(Attributes = "asp-authorize-not")]
    [HtmlTargetElement(Attributes = "asp-authorize-not,asp-roles-not")]
    public class AuthorizeNotTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizeNotTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("asp-roles-not")]
        public string Roles { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext.User;

            // If no attribute value was provided, hide for authenticated users
            if (string.IsNullOrEmpty(Roles))
            {
                if (user.Identity.IsAuthenticated)
                {
                    output.SuppressOutput();
                }
                return;
            }

            // If roles were specified, hide for users in those roles
            if (user.Identity.IsAuthenticated)
            {
                var rolesSplit = Roles.Split(',').Select(r => r.Trim());

                foreach (var role in rolesSplit)
                {
                    if (user.IsInRole(role))
                    {
                        output.SuppressOutput();
                        return;
                    }
                }
            }
        }
    }
}