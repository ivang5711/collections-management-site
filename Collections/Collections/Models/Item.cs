using Collections.Data;
using Microsoft.EntityFrameworkCore;

namespace Collections.Models;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Tag> Tags { get; } = [];

    public string ApplicationUserId { get; set; } = string.Empty;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public ApplicationUser ApplicationUser { get; set; } = new();

    public Collection Collection { get; set; } = new();

    public int CollectionId { get; set; }

    public List<Like> Likes { get; set; } = [];

    public List<Comment> Comments { get; set; } = [];

    public string? ImageLink { get; set; } = null;

    public int LikesTotal { get; set; }
}