namespace Collections.Models
{
    public class LogicalField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Value { get; set; }
        public List<Item> Items { get; set; } = [];
    }
}
