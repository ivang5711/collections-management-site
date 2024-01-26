using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class DateField
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; }
    public DateOnly Value { get; set; }
    public List<Item> Items { get; set; } = [];
}