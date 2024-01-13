using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Collections.Components.Pages;

public partial class CollectionDetails
{
    [Parameter]
    public int Id { get; set; }

    private Collection? collection;
    private List<Item>? itemsBunch;
    private ApplicationUser? ThisUser;
    private bool newItemRequested = false;
    private bool editItemRequested = false;
    private bool deleteItemRequested = false;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";

    private bool themeIsUnique = true;
    private bool newThemeAddFinishedSuccessfully = true;
    private bool newCollectionRequested = false;
    private bool addNewThemeRequested = false;
    private string? TempImg { get; set; } = string.Empty;

    public string? NewTheme { get; set; }
    public List<Theme> Themes { get; set; } = [];

    [SupplyParameterFromForm]
    public ItemCandidate? ItemModel { get; set; }

    public CollectionCandidate? CollectionModel { get; set; }

    public class ItemCandidate
    {
        public string? ImageLink { get; set; }

        [Required]
        public int CollectionId { get; set; }

        [Required]
        public string? Name { get; set; }
    }

    public class CollectionCandidate : Collection
    {
        [Required]
        public new string Name { get; set; }

        [Required]
        public new int ThemeID { get; set; }

        [Required]
        public new string Description { get; set; }
    }

    private void GetThemes()
    {
        using var adc = _contextFactory.CreateDbContext();
        Themes = [.. adc.Themes];
    }

    private void CreateNewTheme()
    {
        using var adc = _contextFactory.CreateDbContext();
        adc.Themes.Add(new Theme { Name = NewTheme! });
        adc.SaveChanges();
    }

    private void RequestNewTheme()
    {
        GetThemes();
        addNewThemeRequested = !addNewThemeRequested;
    }

    private void SubmitNewTheme()
    {
        newThemeAddFinishedSuccessfully = true;
        themeIsUnique = true;
        if (!string.IsNullOrWhiteSpace(NewTheme))
        {
            GetThemes();
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
                CreateNewTheme();
                NewTheme = string.Empty;
                RequestNewTheme();
            }
            else
            {
                newThemeAddFinishedSuccessfully = false;
            }
        }
    }


    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();
        FetchCollectionsDataFromDataSource();
        GetThemes();
        ItemModel ??= new();
        CollectionModel = new();
    }

    private void ToggleNewItemRequestStatus()
    {
        newItemRequested = !newItemRequested;
    }

    private void ToggleEditCollectionRequestStatus()
    {
        TempImg = collection!.ImageLink;
        editItemRequested = !editItemRequested;
    }

    private void ToggleDeleteCollectionRequestStatus()
    {
        deleteItemRequested = !deleteItemRequested;
    }

    private List<Collection> GetCollectionsFromDataSource()
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
            ];
        }
        return t;
    }

    private void CreateItem(ItemCandidate itemCandidate)
    {
        using var adc = _contextFactory.CreateDbContext();
        Item item = new()
        {
            Name = itemCandidate.Name!,
            CollectionId = collection!.Id,
            ImageLink = itemCandidate.ImageLink,
            CreationDateTime = DateTime.UtcNow
        };

        adc.Items.Add(item);
        adc.SaveChanges();
    }

    private void CreateNewItem()
    {
        CreateItem(ItemModel!);
        ToggleNewItemRequestStatus();
        ItemModel ??= new();
        FetchCollectionsDataFromDataSource();
    }

    private void SubmitNewItem()
    {
        ItemModel!.ImageLink = TempImg;
        ItemModel!.CollectionId = collection!.Id;
        CreateNewItem();
        newItemRequested = false;
    }

    private void SubmitEditCollection()
    {
    }

    private void SubmitDeleteCollection()
    {
    }

    private void FetchCollectionsDataFromDataSource()
    {
        itemsBunch = [];
        var t = GetCollectionsFromDataSource().ToList<Collection>();
        collection = t.First(x => x.Id == Id);
        itemsBunch.AddRange(collection.Items.OrderByDescending(u => u.CreationDateTime));
    }

    private async Task CheckAuthorizationLevel()
    {
        GetAuthenticationState();
        if (ThisUser is not null)
        {
            GetUserBlockedStatus(out bool userInRoleBlocked, out bool userIsBlocked);
            if (userInRoleBlocked || userIsBlocked)
            {
                await LogoutCurrentUser();
            }
        }
    }

    private async Task LogoutCurrentUser()
    {
        await _SignInManager.SignOutAsync();
        _navigationManager.NavigateTo(loginPageURL);
    }

    private void GetUserBlockedStatus(out bool userInRoleBlocked, out bool userIsBlocked)
    {
        userInRoleBlocked = Task.Run(() =>
            _UserManager.IsInRoleAsync(ThisUser!, roleBlocked)).Result;
        userIsBlocked = ThisUser!.LockoutEnd is not null;
    }

    private void GetAuthenticationState()
    {
        AuthenticationState authenticationState = Task.Run(() =>
                    _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        ThisUser = Task.Run(() =>
            _UserManager.GetUserAsync(authenticationState.User)).Result;
    }
}