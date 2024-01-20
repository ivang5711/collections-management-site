namespace Collections.Models
{
    public class TextField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public List<Item> Items { get; set; } = [];
    }
}
