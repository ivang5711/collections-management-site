using Bogus;
using Collections.Models;

namespace Collections.Components.TestData;

public class DataGenerator(string locale)
{
    private readonly Faker<Collection> collectionFake =
        new Faker<Collection>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Id = f.IndexFaker + 1;
            u.ItemsIds = Enumerable.Range((f.IndexFaker + 1) * 10, 10).ToList();
            u.TotalItems = u.ItemsIds.Count;
            u.Name = f.Commerce.ProductName();
            u.Description = f.Commerce.ProductDescription();
            u.Theme = f.Commerce.Categories(1)[0];
            u.ImageLink = f.Image.PicsumUrl();
        });

    private readonly Faker<Item> itemFake =
        new Faker<Item>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Id = f.IndexFaker + 1;
            u.Name = f.Commerce.ProductName();
            u.TagIds = [.. f.Make(5, () => f.Database.Random.Int(1, 6))];
            u.Author = f.Person.FullName;
            u.Collection = f.Commerce.ProductName();
            u.CommentsIds = [.. f.Make(10, () => f.Database.Random.Int(1, 500))];
            u.ImageLink = f.Image.PicsumUrl();
        });

    private readonly Faker<Comment> commentFake =
    new Faker<Comment>(locale)
    .StrictMode(false)
    .Rules((f, u) =>
    {
        u.Id = f.IndexFaker + 1;
        u.Name = f.Internet.UserName();
        u.Text = f.Rant.Review();

    });

    public List<Collection> GenerateCollection(int amount, int seed)
    {
        return collectionFake.UseSeed(seed).Generate(amount);
    }

    public List<Item> GenerateItems(int amount, int seed)
    {
        return itemFake.UseSeed(seed).Generate(amount);
    }
    public List<Comment> GenerateComments(int amount, int seed)
    {
        return commentFake.UseSeed(seed).Generate(amount);
    }
}