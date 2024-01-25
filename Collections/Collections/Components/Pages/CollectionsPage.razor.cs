using Collections.Data;
using Collections.Models;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Collections.Components.Pages;

public partial class CollectionsPage
{
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private int maxFileSize;
    private ApplicationUser? thisUser = null;
    private bool addNewThemeRequested = false;
    private bool newCollectionRequested = false;
    private List<Collection>? collections = null;
    private string blobTempDirectory = string.Empty;
    private string blobTempDirectoryPath = string.Empty;
    private bool newThemeAddFinishedSuccessfully = true;
    private string collectionBlobContainerName = string.Empty;
    private string? NewTheme { get; set; }
    private string? UploadedFileName { get; set; }
    public string? ThemeNameChoosen { get; set; }
    private List<Theme> Themes { get; set; } = [];
    private string TempImg { get; set; } = string.Empty;
    private string FileError { get; set; } = string.Empty;

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

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();
        GetConfigurationData();
        SetUpFileTransferManager();
        await InitializeData();
    }

    private void GetConfigurationData()
    {
        collectionBlobContainerName = _configuration
            .GetValue<string>("CollectionBlobContainerName") ?? string.Empty;
        blobTempDirectoryPath = _configuration
            .GetValue<string>("BlobTempDirectoryPath") ?? string.Empty;
        blobTempDirectory = _configuration
            .GetValue<string>("BlobTempDirectory") ?? string.Empty;
        maxFileSize = _configuration.GetValue<int>("MaxFileSize");
    }

    private void SetUpFileTransferManager()
    {
        _fileTransferManager.SetUpMaxFileSize(maxFileSize);
        _fileTransferManager.SetUpWorkingDirectory(blobTempDirectoryPath);
    }

    private async Task InitializeData()
    {
        Model = new();
        ThemeNameChoosen = null;
        GetThemes();
        await GetCollectionsFromDataSource();
    }

    private void GetThemes()
    {
        using var adc = _contextFactory.CreateDbContext();
        Themes = [.. adc.Themes];
    }

    private async Task GetCollectionsFromDataSource()
    {
        await Task.Run(() => CreateData());
    }

    private void CreateData()
    {
        using var adc = _contextFactory.CreateDbContext();
        var t = adc.Collections
            .Include(e => e.Theme)
            .Include(e => e.Items)
            .ToList();
        collections = [.. t.Where(x => x.ApplicationUserId == thisUser!.Id)
            .OrderByDescending(u => u.Items.Count)];
    }

    public async Task UploadFile(InputFileChangeEventArgs e)
    {
        FileError = string.Empty;
        IBrowserFile file = e.File;
        if (file is not null)
        {
            if (file.Size > maxFileSize)
            {
                FileError = "File is too big! The file size is limited to " +
                    $"{maxFileSize / (1024 * 1024)} MB";
                await InitializeData();
                StateHasChanged();
                return;
            }

            try
            {
                UploadedFileName = await _fileTransferManager
                    .SaveFileToDisk(file);
                TempImg = Path.Combine(blobTempDirectory, UploadedFileName);
                await AddCollectionImage();
            }
            catch (Exception ex)
            {
                FileError = $"File Error: {ex}";
                await InitializeData();
            }
        }

        StateHasChanged();
    }

    private async Task AddCollectionImage()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(),
            blobTempDirectoryPath, UploadedFileName!);
        await UploadFileToCloud(path, collectionBlobContainerName);
        AssignNewImageToCollection(collectionBlobContainerName);
    }

    private async Task UploadFileToCloud(string path, string blobContainerName)
    {
        await _blobService.UploadFileBlobAsync(path, UploadedFileName!,
            blobContainerName);
    }

    private void AssignNewImageToCollection(string blobContainerName)
    {
        Model!.ImageLink = _blobService.GetBlobUrl(UploadedFileName!,
            blobContainerName);
    }

    private async Task SubmitNewCollectionForm()
    {
        Model!.ApplicationUserId = thisUser!.Id;
        Model!.ThemeID = Themes.First(x => x.Name == ThemeNameChoosen).Id;
        await CreateNewCollection();
        newCollectionRequested = false;
    }

    private async Task CreateNewCollection()
    {
        CreateCollection(Model!);
        ClearStoredImageFromDisk();
        newCollectionRequested = !newCollectionRequested;
        TempImg = string.Empty;
        await InitializeData();
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

    private string CreateMarkdown(string input)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
        return Markdown.ToHtml(input, pipeline);
    }

    private void ClearStoredImageFromDisk()
    {
        if (Model!.ImageLink is not null)
        {
            _fileTransferManager.DeleteFileFromDisk(UploadedFileName!);
        }
    }

    private void RequestNewCollection()
    {
        GetThemes();
        newCollectionRequested = !newCollectionRequested;
    }

    private async Task CancelRequestNewCollection()
    {
        await InitializeData();
        StateHasChanged();
        newCollectionRequested = !newCollectionRequested;
    }

    private void ResetNewTHemeAddFinishedSuccessfullyStatus()
    {
        newThemeAddFinishedSuccessfully = true;
    }

    private void SubmitNewTheme()
    {
        newThemeAddFinishedSuccessfully = true;
        if (!string.IsNullOrWhiteSpace(NewTheme))
        {
            GetThemes();
            if (CheckIfThemeIsUnique())
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

    private bool CheckIfThemeIsUnique()
    {
        foreach (var theme in Themes)
        {
            if (theme.Name == NewTheme)
            {
                return false;
            }
        }

        return true;
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

    private async Task CheckAuthorizationLevel()
    {
        AuthenticationState authenticationState = Task.Run(() =>
            _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        thisUser = Task.Run(() =>
            _UserManager.GetUserAsync(authenticationState.User)).Result;
        if (thisUser is not null)
        {
            bool userInRoleBlocked = Task.Run(() =>
                _UserManager.IsInRoleAsync(thisUser, roleBlocked)).Result;
            bool userIsBlocked = thisUser.LockoutEnd is not null;
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
}