using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Collections.Components.Pages;

public partial class CollectionsPage
{
    private List<Collection>? collections;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private List<string> WordsToShow { get; set; } = [];
    private ApplicationUser? ThisUser;
    private bool newCollectionRequested = false;
    private string TempImg { get; set; } = string.Empty;
    public List<Theme> Themes { get; set; } = [];

    [SupplyParameterFromForm]
    public CollectionCandidate? Model { get; set; }

    public class CollectionCandidate
    {
        public string? ImageLink { get; set; }
        public string? ApplicationUserId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public int ThemeId { get; set; }

        [Required]
        public string? Description { get; set; }
    }

    private async Task Submit()
    {
        Model!.ImageLink = TempImg;
        Model!.ApplicationUserId = ThisUser!.Id;
        Console.WriteLine(Model?.Name);
        Console.WriteLine(Model?.Description);
        Console.WriteLine(Model?.ImageLink);
        Console.WriteLine(Model?.ThemeId);
        Console.WriteLine(Model?.ApplicationUserId);
        await CreateNewCollection();
        newCollectionRequested = false;
        //StateHasChanged();
        //_navigationManager.Refresh();
    }

    private void RequestNewCollection()
    {
        Themes = GetThemes();
        newCollectionRequested = !newCollectionRequested;
    }

    private async Task CreateNewCollection()
    {
        CreateCollection(Model!);

        newCollectionRequested = !newCollectionRequested;

        TempImg = string.Empty;
        Model ??= new();
        var te = await Task.Run(() => CreateData());
        collections = [.. te];
        foreach (var t in collections)
        {
            t.TotalItems = t.Items.Count;
        }
    }

    private List<Theme> GetThemes()
    {
        List<Theme> res;
        using (var adc = _contextFactory.CreateDbContext())
        {
            res = adc.Themes.ToList();
        }

        return res;
    }

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();

        Model ??= new();

        Themes = GetThemes();

        var te = await Task.Run(() => CreateData());
        collections = [.. te];

        foreach (var t in collections)
        {
            t.TotalItems = t.Items.Count;
        }
    }

    private void CreateCollection(CollectionCandidate collectionCandidate)
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var collection = new Collection
            {
                Name = collectionCandidate.Name,
                ThemeID = collectionCandidate.ThemeId,
                ApplicationUserId = collectionCandidate.ApplicationUserId,
                Description = collectionCandidate.Description,
                ImageLink = collectionCandidate.ImageLink,
            };

            adc.Collections.Add(collection);
            adc.SaveChanges();
        }
    }

    private List<Collection> CreateData()
    {
        List<Collection> temp;
        using (var adc = _contextFactory.CreateDbContext())
        {
            var t = adc.Collections
            .Include(e => e.Theme)
            .Include(e => e.Items)
            .ThenInclude(e => e.Tags)
            .Include(e => e.Items)
            .ThenInclude(e => e.Likes)
            .ToList();
            temp = t.Where(x => x.ApplicationUserId == ThisUser!.Id).ToList();
        }

        return temp;
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
        else
        {
            _navigationManager.NavigateTo("/");
        }
    }
}