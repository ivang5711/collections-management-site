namespace Collections.Models;

public class Comment
{
    public int Id { get; set; }

    public Item Item { get; set; } = new();

    public int ItemId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}