using Collections.Data;

namespace Collections.Models
{
    public class Like
    {
        public int Id { get; set; }

        public ApplicationUser User { get; set; } = new();

        public string UserId { get; set; } = string.Empty; 

        public Item Item { get; set; } = new();

    }
}
