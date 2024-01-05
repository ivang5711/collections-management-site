using Microsoft.AspNetCore.Identity;

namespace Collections.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime LastLoginDate { get; set; }

        public DateTime RegistrationDate { get; set; }
    }

}
