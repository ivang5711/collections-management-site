using Collections.Models;
using Microsoft.AspNetCore.Identity;

namespace Collections.Data
{
    // Add profile data for application users by adding properties to
    // the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime LastLoginDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string FullName { get; set; } = string.Empty;

        public List<Like> Likes { get; set; } = [];

        public ICollection<Collection> Collections { get; } = [];

        public ICollection<Item> Items { get; } = [];

        public ICollection<Comment> Comments { get; } = [];
    }
}