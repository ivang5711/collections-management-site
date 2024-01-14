using Collections.Data;
using Microsoft.EntityFrameworkCore;

namespace Collections.Models
{
    public class Like
    {
        public int Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string ApplicationUserId { get; set; }

        public int ItemId { get; set; }

        public Item Item { get; set; }

    }
}
