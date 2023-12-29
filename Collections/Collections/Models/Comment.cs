namespace Collections.Models;

public class Comment
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Text { get; set; }
}