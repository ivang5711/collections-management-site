namespace Collections.Models;

public partial class Collection(int id, List<int> items, string name, string description, string theme, string? imageLink = null)
{
    public int Id { get; set; } = id;
    public List<int> Items { get; set; } = items;

    public int TotalItems { get; } = items.Count;

    public  string Name { get; set; } = name;

    public  string Description { get; set; } = description;

    public  string Theme { get; set; } = theme;

    public string? ImageLink { get; set; } = imageLink;
}