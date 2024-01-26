using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class Tag
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;
    public List<Item> Items { get; set; } = [];
}