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
    private int maxFileSize;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private bool newThemeAddFinishedSuccessfully = true;
    private bool newCollectionRequested = false;
    private bool addNewThemeRequested = false;
    public string? ThemeNameChoosen { get; set; }
    private List<Collection>? collections = null;
    private ApplicationUser? ThisUser = null;
    private string TempImg { get; set; } = string.Empty;
    public string FileError { get; set; } = string.Empty;
    public string? NewTheme { get; set; }

    public List<Theme> Themes { get; set; } = [];

    public string? UploadedFileName { get; set; }

    private string blobTempDirectory = string.Empty;
    private string collectionBlobContainerName = string.Empty;
    private string blobTempDirectoryPath = string.Empty;

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

    private void AssignNewImageToCollection(string blobContainerName)
    {
        Model!.ImageLink = _blobService.GetBlobUrl(UploadedFileName!,
            blobContainerName);
    }

    private async Task UploadFileToCloud(string path, string blobContainerName)
    {
        await _blobService.UploadFileBlobAsync(path, UploadedFileName!,
            blobContainerName);
    }

    private string CreateMarkdown(string input)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
        return Markdown.ToHtml(input, pipeline);
    }

    private async Task SubmitNewCollectionForm()
    {
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

    private async Task CreateNewCollection()
    {
        CreateCollection(Model!);
        ClearStoredImageFromDisk();
        newCollectionRequested = !newCollectionRequested;
        TempImg = string.Empty;
        await InitializeData();
    }

    private void ClearStoredImageFromDisk()
    {
        if (Model!.ImageLink is not null)
        {
            _fileTransferManager.DeleteFileFromDisk(UploadedFileName!);
        }
    }

    private async Task GetCollectionsFromDataSource()
    {
        await Task.Run(() => CreateData());
    }

    private void GetThemes()
    {
        using var adc = _contextFactory.CreateDbContext();
        Themes = [.. adc.Themes];
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