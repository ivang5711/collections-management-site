using Collections.Components.TestData;
using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Collections.Components.Pages;

public partial class CollectionDetails
{
    [Parameter]
    public int Id { get; set; }

    private Collection? collection;
    private List<Item>? itemsBunch;

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

        itemsBunch = [];

        var t = CreateData2().ToList<Collection>();
        collection = t.First(x => x.Id == Id);
        itemsBunch.AddRange(collection.Items);
    }

    private void AddItem()
    {
        CreateItem();
    }

    private List<Collection> CreateData2()
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

    private void CreateItem()
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            Item item = new()
            {
                Name = "A walk in city center",
                CollectionId = collection!.Id,
                ImageLink = "https://th.bing.com/th/id/OIP.ZPvEhHtbC40JGgd3XzpQ9AHaE8?rs=1&pid=ImgDetMain",
                
                
            };

            adc.Items.Add(item);
            adc.SaveChanges();
        }
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