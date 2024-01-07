namespace Collections.Models;

public class Collection
{
    public Collection()
    {
        Id = 0;
        ItemsIds = [];
        TotalItems = 0;
        Name = string.Empty;
        Description = string.Empty;
        Theme = string.Empty;
        ImageLink = string.Empty;
    }

    public Collection(int id, List<int> itemsIds, string name,
        string description, string theme, string? imageLink = null)
    {
        Id = id;
        ItemsIds = itemsIds;
        TotalItems = itemsIds.Count;
        Name = name;
        Description = description;
        Theme = theme;
        ImageLink = imageLink;
    }

    public int Id { get; set; }
    public List<int> ItemsIds { get; set; }

    public int TotalItems { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Theme { get; set; }

    public string? ImageLink { get; set; }
}