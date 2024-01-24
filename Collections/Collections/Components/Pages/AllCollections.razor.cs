using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Collections.Components.Pages;

public partial class AllCollections
{
    private List<Collection>? collections;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private ApplicationUser? ThisUser;
    public List<Theme> Themes { get; set; } = [];

    private void SortItemsDescending()
    {
        List<Collection> SortedList = collections!.OrderByDescending(x => x.CreationDateTime).ToList();

        collections!.Clear();
        collections.AddRange(SortedList);
        StateHasChanged();
    }

    private void SortItemsAscending()
    {
        List<Collection> SortedList = collections!.OrderBy(x => x.CreationDateTime).ToList();

        collections!.Clear();
        collections.AddRange(SortedList);
        StateHasChanged();
    }

    private List<Theme> GetThemes()
    {
        List<Theme> res;
        using (var adc = _contextFactory.CreateDbContext())
        {
            res = [.. adc.Themes];
        }

        return res;
    }

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();

        Themes = GetThemes();

        var te = await Task.Run(() => CreateData());
        collections = [.. te];

        foreach (var t in collections)
        {
            t.TotalItems = t.Items.Count;
        }
    }

    private List<Collection> CreateData()
    {
        List<Collection> t;
        using (var adc = _contextFactory.CreateDbContext())
        {
            t =
            [
                .. adc.Collections
                           .Include(e => e.Theme)
                           .Include(e => e.Items)
                           .ThenInclude(e => e.Tags)
                           .Include(e => e.Items)
                           .ThenInclude(e => e.Likes)
,
            ];
        }

        var res = collections = [.. t.OrderByDescending(u => u.Items.Count)];

        return res;
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