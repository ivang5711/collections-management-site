using Collections.Data;
using Collections.Models;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Collections.Components.Pages;

public partial class CollectionsPage
{
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private bool themeIsUnique = true;
    private bool newThemeAddFinishedSuccessfully = true;
    private bool newCollectionRequested = false;
    private bool addNewThemeRequested = false;
    public string ThemeNameChoosen { get; set; }
    private List<Collection>? collections = null;
    private ApplicationUser? ThisUser = null;
    private string TempImg { get; set; } = string.Empty;
    public string? NewTheme { get; set; }
    public List<Theme> Themes { get; set; } = [];

    [SupplyParameterFromForm]
    public CollectionCandidate? Model { get; set; }

    public class CollectionCandidate : Collection
    {
        [Required]
        public new string? Name { get; set; }

        [Required]
        public new int ThemeID { get; set; }

        [Required]
        public new string? Description { get; set; }
    }

    private string CreateMarkdown(string input)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
        return Markdown.ToHtml(input, pipeline);
    }

    private async Task SubmitNewCollectionForm()
    {
        Model!.ImageLink = TempImg;
        Model!.ApplicationUserId = ThisUser!.Id;
        Model!.ThemeID = Themes.First(x => x.Name == ThemeNameChoosen).Id;
        await CreateNewCollection();
        newCollectionRequested = false;
    }

    private void RequestNewCollection()
    {
        GetThemes();
        newCollectionRequested = !newCollectionRequested;
    }

    private void ResetNewTHemeAddFinishedSuccessfullyStatus()
    {
        newThemeAddFinishedSuccessfully = true;
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
        await Task.Run(() => CreateData());
        if (collections is not null)
        {
            foreach (Collection t in collections)
            {
                t.TotalItems = t.Items.Count;
            }
        }
    }

    private void GetThemes()
    {
        using var adc = _contextFactory.CreateDbContext();
        Themes = [.. adc.Themes];
    }

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();
        Model ??= new();
        GetThemes();
        await GetCollectionsFromDataSource();
    }

    private void CreateNewTheme()
    {
        using var adc = _contextFactory.CreateDbContext();
        adc.Themes.Add(new Theme { Name = NewTheme! });
        adc.SaveChanges();
    }

    private void CreateCollection(CollectionCandidate collectionCandidate)
    {
        using var adc = _contextFactory.CreateDbContext();
        var collection = new Collection
        {
            Name = collectionCandidate!.Name!,
            ThemeID = collectionCandidate!.ThemeID,
            ApplicationUserId = collectionCandidate!.ApplicationUserId!,
            Description = CreateMarkdown(collectionCandidate!.Description!),
            ImageLink = collectionCandidate.ImageLink,
            CreationDateTime = DateTime.UtcNow
        };

        adc.Collections.Add(collection);
        adc.SaveChanges();
    }

    private void CreateData()
    {
        using var adc = _contextFactory.CreateDbContext();
        var t = adc.Collections
            .Include(e => e.Theme)
            .Include(e => e.Items)
            .ToList();
        collections = [.. t.Where(x => x.ApplicationUserId == ThisUser!.Id)
            .OrderByDescending(u => u.Items.Count)];
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