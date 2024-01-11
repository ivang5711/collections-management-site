using Bogus;
using Collections.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Collections.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Theme> Themes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);



            builder.Entity<Collection>().ToTable("Collections");
            builder.Entity<Item>().ToTable("Items");
            builder.Entity<Comment>().ToTable("Comments");
            builder.Entity<Tag>().ToTable("Tags");
            builder.Entity<Like>().ToTable("Likes");
            builder.Entity<Theme>().ToTable("Themes");

            //builder.Entity<ApplicationUser>().HasMany(e => e.Likes)
            //    .WithOne(e => e.User)
            //    .HasForeignKey(e => e.UserId)
            //    .HasPrincipalKey(e => e.Id);

            //builder.Entity<Collection>()
            //                    .HasMany(e => e.Items)
            //                    .WithOne(e => e.Collection)
            //                    .HasForeignKey(e => e.Id)
            //                    .HasPrincipalKey(e => e.Id);


            //builder.Entity<Item>().HasMany(e => e.Tags)
            //                      .WithOne(e => e.Item)
            //                      .HasForeignKey(e => e.Id)
            //                      .HasPrincipalKey(e => e.Id);

            //builder.Entity<Item>().HasMany(e => e.Comments)
            //                      .WithOne(e => e.Item)
            //                      .HasForeignKey(e => e.Id)
            //                      .HasPrincipalKey(e => e.Id);

            //builder.Entity<Item>().HasMany(e => e.Likes)
            //                      .WithOne(e => e.Item)
            //                      .HasForeignKey(e => e.Id)
            //                      .HasPrincipalKey(e => e.Id);
        }
    }
}