using Collections.Components.TestData;
using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
        List<Item> temp = [];
        foreach (var t in te)
        {
            t.TotalItems = t.Items.Count;
            temp.AddRange(t.Items);
        }

        items.AddRange(temp.OrderByDescending(u => u.CreationDateTime).Take(5));
        collections = te.OrderByDescending(x => x.TotalItems).Take(5).ToList();
        tagsGenerated = [];
        List<Tag> k2;
        using (var adc = _contextFactory.CreateDbContext())
        {
            k2 = adc.Tags.AsEnumerable<Tag>().ToList<Tag>();
        }

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
        CreateCollection();
    }

    private string GetAuthor(Collection collection)
    {
        string res = string.Empty;
        using (var adc = _contextFactory.CreateDbContext())
        {
            res = adc.Users.First(x => x.Id == collection.ApplicationUserId).FullName;
        }

        return res;
    }

    private void CreateCollection()
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var collection = new Collection
            {
                Name = "My awesome weekend",
                ThemeID = 2,
                ApplicationUserId = ThisUser!.Id,
                Description = "Photos and memories from the summer vacation 2 " +
                    "to south east Asia. Wonderful landscapes and amazing views. 2 " +
                    "The best food in the world and much more.",

                ImageLink = "https://1.bp.blogspot.com/-2FODK09wE9g/WZA3YXTPTJI/AAAAAAAAAQA/JMZr20FMOpYKoCGS33GQToQVO2_1y_8XgCLcBGAs/s1600/Vacation%2BPostcard%2BRecalculating.jpg",
            };

            adc.Collections.Add(collection);
            adc.SaveChanges();
        }
    }

    private void PopulateThemes()
    {
        using (var adc = _contextFactory.CreateDbContext())
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
        using (var adc = _contextFactory.CreateDbContext())
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
        List<Collection> t;
        using (var adc = _contextFactory.CreateDbContext())
        {
            t = adc.Collections
            .Include(e => e.Theme)
            .Include(e => e.Items)
            .ThenInclude(e => e.Tags)
            .Include(e => e.Items)
            .ThenInclude(e => e.Likes)
            .ToList();
        }

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