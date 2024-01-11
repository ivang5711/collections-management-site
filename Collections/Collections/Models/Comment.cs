using Collections.Data;
using Microsoft.EntityFrameworkCore;

namespace Collections.Models;

public class Comment
{
    public int Id { get; set; }

    public Item Item { get; set; } = new();

    public int ItemId { get; set; }

    public string ApplicationUserId { get; set; } = string.Empty;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public ApplicationUser ApplicationUser { get; set; } = new();

    public string Text { get; set; } = string.Empty;
}