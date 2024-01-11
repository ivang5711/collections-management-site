using Collections.Data;

namespace Collections.Models
{
    public class Like
    {
        public int Id { get; set; }

        public ApplicationUser User { get; set; } = new();

        public string UserId { get; set; } = string.Empty; 

        public List<Item> Items { get; set; } = [];

    }
}
