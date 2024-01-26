using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class LogicalField
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; }

    public bool Value { get; set; }
    public List<Item> Items { get; set; } = [];
}