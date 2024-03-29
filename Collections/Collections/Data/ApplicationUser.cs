using Collections.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Data;

// Add profile data for application users by adding properties to
// the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public DateTime LastLoginDate { get; set; }

    public DateTime RegistrationDate { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string FullName { get; set; } = string.Empty;

    public List<Like> Likes { get; } = [];

    public ICollection<Collection> Collections { get; } = [];

    public ICollection<Comment> Comments { get; } = [];
}