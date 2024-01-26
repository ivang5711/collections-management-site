using Collections.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class Comment
{
    public int Id { get; set; }

    public Item Item { get; set; }

    public int ItemId { get; set; }

    public string ApplicationUserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public ApplicationUser ApplicationUser { get; set; }

    [Column(TypeName = "varchar(300)")]

    public string Text { get; set; } = string.Empty;

    public DateTime CreationDateTime { get; set; }
}