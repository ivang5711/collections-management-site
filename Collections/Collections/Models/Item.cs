namespace Collections.Models;

public class Item(int id, string name, List<int> tagIds, string author, string collection, Dictionary<string, bool>? likes = null, List<int>? commentsIds = null, string? imageLink = null)
{
    public int Id { get; set; } = id;

    public string Name { get; set; } = name;

    public List<int> TagIds { get; set; } = tagIds;

    public string Author { get; set; } = author;

    public string Collection { get; set; } = collection;

    public Dictionary<string, bool>? Likes { get; set; } = likes;

    public List<int>? CommentsIds { get; set; } = commentsIds;

    public string? ImageLink { get; set; } = imageLink;
}