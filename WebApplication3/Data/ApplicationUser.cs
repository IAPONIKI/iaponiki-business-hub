using Microsoft.AspNetCore.Identity;

namespace WebApplication3.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Department { get; set; }
    }
}