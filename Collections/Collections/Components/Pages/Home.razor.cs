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
            PopulateTable();
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
                Name = "My awesome collection",
                ThemeID = 6,
                Items = new List<Item>
                    {
                        new Item
                        {
                            Name = "Unusual keyboard",
                            ApplicationUser = ThisUser!,
                            ImageLink = "https://th.bing.com/th/id/R.9b2a3fd769228fb57123b60d09cb25d7?rik=WrrDpbHet61rCg&riu=http%3a%2f%2fupload.wikimedia.org%2fwikipedia%2fcommons%2f9%2f9f%2fApple_(Standard)_Keyboard_M0116.jpg&ehk=%2fEGwXB3aHJoiSK0PyIntdA4oLRO0LCKPNVamfIYI5gY%3d&risl=&pid=ImgRaw&r=0",
                            Comments = new List<Comment>
                            {
                                new Comment { Name = "Joe Doe", Text = "This comment is awesome and amazing!" },
                                new Comment { Name = "Billy Jane", Text = "The best product on the market!!! No doubt!" },
                            }
                        }

                    },
                ImageLink = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.nytimes.com%2Fwirecutter%2Freviews%2Fthe-best-bluetooth-keyboard%2F&psig=AOvVaw0KxCHDSN3VGHnV0nEis_oT&ust=1705006685182000&source=images&cd=vfe&opi=89978449&ved=0CBEQjRxqFwoTCNi6soDb04MDFQAAAAAdAAAAABAQ",

            };

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
