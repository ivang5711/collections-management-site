using Collections.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Collections.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Comment> Comments { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<Collection> Collections { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<Theme> Themes { get; set; }

    public DbSet<Like> Likes { get; set; }

    public DbSet<NumericalField> NumericalFields { get; set; }

    public DbSet<StringField> StringFields { get; set; }

    public DbSet<TextField> TextFields { get; set; }

    public DbSet<LogicalField> LogicalFields { get; set; }

    public DbSet<DateField> DateFields { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Collection>().ToTable("Collections");
        builder.Entity<Item>().ToTable("Items");
        builder.Entity<Comment>().ToTable("Comments");
        builder.Entity<Tag>().ToTable("Tags");
        builder.Entity<Like>().ToTable("Likes");
        builder.Entity<Theme>().ToTable("Themes");
        builder.Entity<NumericalField>().ToTable("NumericalFields");
        builder.Entity<StringField>().ToTable("StringFields");
        builder.Entity<TextField>().ToTable("TextFields");
        builder.Entity<LogicalField>().ToTable("LogicalFields");
        builder.Entity<DateField>().ToTable("DateFields");
        builder.Entity<Like>()
            .HasIndex(p => new { p.ApplicationUserId, p.ItemId })
            .IsUnique(true);
    }
}