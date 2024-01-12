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
    private ApplicationUser? ThisUser;
    private bool themeIsUnique = true;
    private bool newThemeAddFinishedSuccessfully = true;
    private bool newCollectionRequested = false;
    private bool addNewThemeRequested = false;
    private string TempImg { get; set; } = string.Empty;
    public List<Theme> Themes { get; set; } = [];

    public string? NewTheme { get; set; }

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
    }

    private void RequestNewCollection()
    {
        Themes = GetThemes();
        newCollectionRequested = !newCollectionRequested;
    }

    private void ResetNewTHemeAddFinishedSuccessfullyStatus()
    {
        newThemeAddFinishedSuccessfully = true;
    }

    private void RequestNewTheme()
    {
        Themes = GetThemes();
        addNewThemeRequested = !addNewThemeRequested;
    }

    private void SubmitNewTheme()
    {
        newThemeAddFinishedSuccessfully = true;
        themeIsUnique = true;
        if (!string.IsNullOrWhiteSpace(NewTheme))
        {
            foreach (var theme in Themes)
            {
                if (theme.Name == NewTheme)
                {
                    themeIsUnique = false;
                    break;
                }
            }

            if (themeIsUnique)
            {
                Console.WriteLine("New theme is: " + NewTheme);
                CreateNewTheme();
                NewTheme = string.Empty;
                RequestNewTheme();
            }
            else
            {
                newThemeAddFinishedSuccessfully = false;
                Console.WriteLine("Theme is not unique");
            }
        }
    }

    private async Task CreateNewCollection()
    {
        CreateCollection(Model!);
        newCollectionRequested = !newCollectionRequested;
        TempImg = string.Empty;
        Model ??= new();
        await GetCollectionsFromDataSource();
    }

    private async Task GetCollectionsFromDataSource()
    {
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
            res = [.. adc.Themes];
        }

        return res;
    }

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();
        Model ??= new();
        Themes = GetThemes();
        await GetCollectionsFromDataSource();
    }

    private void CreateNewTheme()
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var theme = new Theme
            {
                Name = NewTheme!
            };

            adc.Themes.Add(theme);
            adc.SaveChanges();
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