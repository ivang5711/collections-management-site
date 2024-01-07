namespace Collections.Models;

public class Item
{
    public Item()
    {
        Name = string.Empty;
        TagIds = [];
        Author = string.Empty;
        Collection = string.Empty;
        Likes = null;
        CommentsIds = null;
        ImageLink = null;
    }

    public Item(int id, string name, List<int> tagIds, string author,
        string collection, Dictionary<string, bool>? likes = null,
        List<int>? commentsIds = null, string? imageLink = null)
    {
        Id = id;
        Name = name;
        TagIds = tagIds;
        Author = author;
        Collection = collection;
        Likes = likes;
        CommentsIds = commentsIds;
        ImageLink = imageLink;
        LikesTotal = Likes?.Keys.Count > 0 ? Likes!.Keys.Count : 0;
    }

    public required int Id { get; set; }

    public string Name { get; set; }

    public List<int> TagIds { get; set; }

    public string Author { get; set; }

    public string Collection { get; set; }

    public Dictionary<string, bool>? Likes { get; set; }

    public List<int>? CommentsIds { get; set; }

    public string? ImageLink { get; set; }

    public int LikesTotal { get; set; }
}