namespace Collections.Models;

public partial class Collection
{
    public List<Item> Items { get; set; } = [];

    public int TotalItems { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string Theme { get; set; }

    public string? ImageLink { get; set; }
}