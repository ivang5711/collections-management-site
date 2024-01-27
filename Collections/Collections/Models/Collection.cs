using Collections.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class Collection
{
    public int Id { get; set; }
    public List<Item> Items { get; set; } = [];

    public string ApplicationUserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public ApplicationUser ApplicationUser { get; set; }

    public int TotalItems { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Theme? Theme { get; set; }

    public int ThemeID { get; set; }

    [Column(TypeName = "varchar(200)")]
    public string? ImageLink { get; set; } = null;

    public DateTime CreationDateTime { get; set; }
}