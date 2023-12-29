namespace Collections.Models;
public partial class Item
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public List<int> TagIds { get; set; } = [];

    public required string Author { get; set; }

    public required string Collection { get; set; }

    public Dictionary<string, bool>? Likes { get; set; }

    public List<Comment>? Comments { get; set; }
}