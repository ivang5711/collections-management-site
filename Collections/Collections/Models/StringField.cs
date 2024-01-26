using System.ComponentModel.DataAnnotations.Schema;

namespace Collections.Models;

public class StringField
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; }

    [Column(TypeName = "varchar(200)")]
    public string Value { get; set; }
    public List<Item> Items { get; set; } = [];
}