namespace Collections.Models;

/// <summary>
/// The class represents a collection item.
/// </summary>
/// <param name="id"></param>
/// <param name="name"></param>
/// <param name="tagIds"></param>
/// <param name="author"></param>
/// <param name="collection"></param>
/// <param name="likes"></param>
/// <param name="comments"></param>
public partial class Item(int id, string name, List<int> tagIds, string author, string collection, Dictionary<string, bool>? likes = null, List<Comment>? comments = null)
{
    public int Id { get; set; } = id;

    public string Name { get; set; } = name;

    public List<int> TagIds { get; set; } = tagIds;

    public string Author { get; set; } = author;

    public string Collection { get; set; } = collection;

    public Dictionary<string, bool>? Likes { get; set; } = likes;

    public List<Comment>? Comments { get; set; } = comments;

}