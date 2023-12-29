namespace Collections.Models;

public partial class Item(int id, string name, List<int> tagIds, string author, string collection, Dictionary<string, bool>? likes = null, List<Comment>? comments = null, string? imageLink = null)
{
    public int Id { get; set; } = id;

    public string Name { get; set; } = name;

    public List<int> TagIds { get; set; } = tagIds;

    public string Author { get; set; } = author;

    public string Collection { get; set; } = collection;

    public Dictionary<string, bool>? Likes { get; set; } = likes;

    public List<Comment>? Comments { get; set; } = comments;

    public string? ImageLink { get; set; } = imageLink;
}