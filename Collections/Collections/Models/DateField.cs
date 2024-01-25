namespace Collections.Models;

public class DateField
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateOnly Value { get; set; }
    public List<Item> Items { get; set; } = [];
}