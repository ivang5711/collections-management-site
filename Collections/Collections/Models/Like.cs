using Collections.Data;

namespace Collections.Models
{
    public class Like
    {
        public int Id { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public string ApplicationUserId { get; set; }

        public List<Item> Items { get; set; } = [];

    }
}
