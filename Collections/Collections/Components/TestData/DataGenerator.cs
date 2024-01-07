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
            u.ItemsIds = Enumerable.Range((f.IndexFaker + 1) * 10, 10)
                .ToList();
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
            u.CommentsIds = [.. f.Make(10, () => f.Database
                .Random.Int(1, 500))];
            u.ImageLink = f.Image.PicsumUrl();
            u.Likes = GenerateLikes(124, 123);
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

    private static readonly Faker<Dictionary<string, bool>> likesFake =
        new Faker<Dictionary<string, bool>>()
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
            u.Add(f.Person.Random.Word(), true);
        });

    private readonly Faker<PersonModel> userFake =
        new Faker<PersonModel>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Id = f.IndexFaker + 1;
            u.Name = f.Person.FullName;
            u.Email = f.Person.Email;
            u.RegistrationDate = f.Person.DateOfBirth;
            u.LastLoginDate = f.Date.Between(DateTime.MinValue, DateTime.Now);
            u.Status = f.Random.Bool();
            u.IsAdmin = f.Random.Bool();
        });

    private readonly Faker<Tag> tagFake =
        new Faker<Tag>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Id = f.IndexFaker + 1;
            u.Name = f.Lorem.Word();
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

    public static Dictionary<string, bool> GenerateLikes(int amount, int seed)
    {
        return likesFake.UseSeed(seed).Generate();
    }

    public List<PersonModel> GenerateUsers(int amount, int seed)
    {
        return userFake.UseSeed(seed).Generate(amount);
    }
    public List<Tag> GenerateTags(int amount, int seed)
    {
        return tagFake.UseSeed(seed).Generate(amount);
    }
}