namespace Collections.Models;

public class Collection
{
    public int Id { get; set; }
    public List<Item> Items { get; set; } = [];
        
    public int TotalItems { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Theme? Theme { get; set; }

    public int ThemeID { get; set; }

    public string? ImageLink { get; set; } = null;
}