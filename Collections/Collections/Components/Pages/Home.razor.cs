using Collections.Components.TestData;
using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Collections.Components.Pages;

public partial class Home
{
    private List<Collection>? collections;
    private List<Item> items = [];
    private List<Tag>? tagsGenerated;
    private readonly DataGenerator dg = new("en");
    private const int seed = 123;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private List<string> WordsToShow { get; set; } = [];
    private ApplicationUser? ThisUser;

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();

        var te = await Task.Run(() => CreateData());
        collections = te.Take(5).ToList();

        List<Item> temp = [];

        foreach (var t in collections)
        {
            t.TotalItems = t.Items.Count;
            temp.AddRange(t.Items);
        }

        items.AddRange(temp.Take(5));

        tagsGenerated = [];
        var k2 = adc.Tags.AsEnumerable<Tag>().ToList<Tag>();
        tagsGenerated.AddRange(k2);

        WordsToShow.Clear();
        foreach (var tag in tagsGenerated)
        {
            WordsToShow.Add(tag.Name);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            //PopulateTable();
        }
    }

    private void PopulateTable()
    {
        //PopulateThemes();
        //PopulateTags();
        //CreateCollection();
    }

    private void CreateCollection()
    {
        using (adc)
        {
            var collection = new Collection
            {
                Name = "My awesome weekend",
                ThemeID = 2,
                ApplicationUserId = ThisUser!.Id,
                Description = "Photos and memories from the summer vacation " +
                    "to south east Asia. Wonderful landscapes and amazing views. " +
                    "The best food in the world and much more.",
                Items =
                    [
                        new Item
                        {
                            Name = "The Great Waterfall",
                            ApplicationUserId = ThisUser!.Id,
                            ImageLink = "https://images.pexels.com/photos/2406395/pexels-photo-2406395.jpeg?cs=srgb&dl=pexels-avery-nielsenwebb-2406395.jpg&fm=jpg",
                            Comments =
                            [
                                new()
                                {
                                    ApplicationUserId = ThisUser!.Id,
                                    Text = "This item is awesome and amazing!"
                                },
                                new()
                                {
                                    ApplicationUserId = ThisUser!.Id,
                                    Text = "The best product on the market!!!"
                                },
                            ]
                        },
                        new Item
                        {
                            Name = "Street Food Gem",
                            ApplicationUserId = ThisUser!.Id,
                            ImageLink = "https://th.bing.com/th/id/OIP.Ju-1rTVwR5-rizOKEgLELAHaE8?rs=1&pid=ImgDetMain",
                            Comments =
                            [
                                new()
                                {
                                    ApplicationUserId = ThisUser!.Id,
                                    Text = "I wish to eat it every day"
                                },
                                new()
                                {
                                    ApplicationUserId = ThisUser!.Id,
                                    Text = "Too good to be true"
                                },
                                new()
                                {
                                    ApplicationUserId = ThisUser!.Id,
                                    Text = "It was not bad at all"
                                },
                            ]
                        }

                    ],
                ImageLink = "https://1.bp.blogspot.com/-2FODK09wE9g/WZA3YXTPTJI/AAAAAAAAAQA/JMZr20FMOpYKoCGS33GQToQVO2_1y_8XgCLcBGAs/s1600/Vacation%2BPostcard%2BRecalculating.jpg",
            };

            collection.TotalItems = collection.Items.Count;

            adc.Collections.Add(collection);
            adc.SaveChanges();
        }
    }

    private void PopulateThemes()
    {
        using (adc)
        {
            List<Theme> theme = [
                new Theme { Name = "Movies" },
                new Theme { Name = "Travel" },
                new Theme { Name = "Music" },
                new Theme { Name = "Sport" },
                new Theme { Name = "Computers" },
                new Theme { Name = "Photo" },
                new Theme { Name = "Cars" },
                new Theme { Name = "Space" },
                new Theme { Name = "Fishing" },
                new Theme { Name = "Art" },
            ];

            adc.Themes.AddRange(theme);
            adc.SaveChanges();
        }
    }

    private void PopulateTags()
    {
        using (adc)
        {
            List<Tag> tags = [
                new Tag { Name = "Chicago" },
                new Tag { Name = "Rocket" },
                new Tag { Name = "Beast" },
                new Tag { Name = "Pencils" },
                new Tag { Name = "Best" },
                new Tag { Name = "Awesome" },
                new Tag { Name = "Lame" },
                new Tag { Name = "Lunch" },
                new Tag { Name = "Vacation" },
                new Tag { Name = "Holiday" },
            ];

            adc.Tags.AddRange(tags);
            adc.SaveChanges();
        }
    }

    private List<Collection> CreateData()
    {
        var t = adc.Collections
        .Include(e => e.Theme)
        .Include(e => e.Items)
        .ThenInclude(e => e.Tags)
        .Include(e => e.Items)
        .ThenInclude(e => e.Likes)
        .ToList();
        return t;
    }

    private async Task CheckAuthorizationLevel()
    {
        AuthenticationState authenticationState = Task.Run(() =>
            _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        ThisUser = Task.Run(() =>
            _UserManager.GetUserAsync(authenticationState.User)).Result;
        if (ThisUser is not null)
        {
            bool userInRoleBlocked = Task.Run(() =>
                _UserManager.IsInRoleAsync(ThisUser, roleBlocked)).Result;
            bool userIsBlocked = ThisUser.LockoutEnd is not null;
            if (userInRoleBlocked || userIsBlocked)
            {
                await _SignInManager.SignOutAsync();
                _navigationManager.NavigateTo(loginPageURL);
            }
        }
    }
}