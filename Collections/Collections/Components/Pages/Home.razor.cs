using Collections.Components.TestData;
using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Collections.Components.Pages;

public partial class Home
{
    private List<Collection>? collections;
    private List<Item>? items;
    private List<Tag>? tagsGenerated;
    private readonly DataGenerator dg = new("en");
    private const int seed = 123;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private List<string> WordsToShow { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();

        collections = await Task.Run(() => CreateData());
        items = await Task.Run(() => CreateItems());
        tagsGenerated = await Task.Run(() => CreateTags());
        WordsToShow.Clear();
        foreach (var tag in tagsGenerated)
        {
            WordsToShow.Add(tag.Name);
        }
    }

    private List<Collection> CreateData()
    {
        return dg.GenerateCollection(5, seed);
    }

    private List<Item> CreateItems()
    {
        return dg.GenerateItems(5, seed * 100);
    }

    private List<Tag> CreateTags()
    {
        return dg.GenerateTags(10, seed);
    }

    private async Task CheckAuthorizationLevel()
    {
        AuthenticationState authenticationState = Task.Run(() =>
            _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        ApplicationUser? currentUser = Task.Run(() =>
            _UserManager.GetUserAsync(authenticationState.User)).Result;
        if (currentUser is not null)
        {
            bool userInRoleBlocked = Task.Run(() =>
                _UserManager.IsInRoleAsync(currentUser, roleBlocked)).Result;
            bool userIsBlocked = currentUser.LockoutEnd is not null;
            if (userInRoleBlocked || userIsBlocked)
            {
                await _SignInManager.SignOutAsync();
                _navigationManager.NavigateTo(loginPageURL);
            }
        }
    }
}