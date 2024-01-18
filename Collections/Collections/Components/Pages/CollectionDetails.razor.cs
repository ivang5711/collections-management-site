using Collections.Data;
using Collections.Models;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Collections.Components.Pages;

public partial class CollectionDetails
{
    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromForm]
    public ItemCandidate? ItemModel { get; set; }

    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private int maxFileSize;
    private Collection? collection;
    private List<Item>? itemsBunch;
    private ApplicationUser? ThisUser;
    private bool themeIsUnique = true;
    private bool newItemRequested = false;
    private bool editItemRequested = false;
    private bool deleteItemRequested = false;
    private bool addNewThemeRequested = false;
    private bool collectionChangeRequestValid = true;
    private bool newThemeAddFinishedSuccessfully = true;

    private string blobTempDirectory = string.Empty;
    private string collectionBlobContainerName = string.Empty;
    private string itemBlobContainerName = string.Empty;
    private string blobTempDirectoryPath = string.Empty;

    public string? NewTheme { get; set; }
    public string? ThemeNameChoosen { get; set; }
    public string? UploadedFileName { get; set; }
    public Collection? CollectionModel { get; set; }
    public List<Theme> Themes { get; set; } = [];
    private string? TempImg { get; set; } = string.Empty;

    public string FileError { get; set; } = string.Empty;

    public class ItemCandidate
    {
        public string? ImageLink { get; set; }

        [Required]
        public int CollectionId { get; set; }

        [Required]
        public string? Name { get; set; }
    }

    private void GetConfigurationData()
    {
        collectionBlobContainerName = _configuration
            .GetValue<string>("CollectionBlobContainerName") ?? string.Empty;
        itemBlobContainerName = _configuration
            .GetValue<string>("itemBlobContainerName") ?? string.Empty;
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

    public async Task UploadFile(InputFileChangeEventArgs e)
    {
        RemovePreviousPhotoIfExists();
        FileError = string.Empty;
        IBrowserFile file = e.File;
        if (file is not null)
        {
            if (file.Size > maxFileSize)
            {
                FileError = "File is too big! The file size is limited to " +
                    $"{maxFileSize / (1024 * 1024)} MB";
                InitializeData();
                StateHasChanged();
                return;
            }

            try
            {
                UploadedFileName = await _fileTransferManager
                    .SaveFileToDisk(file);
                TempImg = Path.Combine(blobTempDirectory, UploadedFileName);
                await AddItemImage();
            }
            catch (Exception ex)
            {
                FileError = $"File Error: {ex}";
                InitializeData();
            }
        }

        StateHasChanged();
    }

    private void RemovePreviousPhotoIfExists()
    {
        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            _fileTransferManager.DeleteFileFromDisk(UploadedFileName);
        }
    }










    private async Task AddItemImage()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(),
            blobTempDirectoryPath, UploadedFileName!);
        await UploadFileToCloud(path, itemBlobContainerName);
        AssignNewImageToItem(itemBlobContainerName);
        //await DeleteOldPhotoFromCloud();
        //_fileTransferManager.DeleteFileFromDisk(UploadedFileName!);
    }

    private async Task UploadFileToCloud(string path, string blobContainerName)
    {
        await _blobService.UploadFileBlobAsync(path, UploadedFileName!,
            blobContainerName);
    }

    private void AssignNewImageToItem(string blobContainerName)
    {
        ItemModel!.ImageLink = _blobService.GetBlobUrl(UploadedFileName!,
            blobContainerName);
    }

    //private async Task DeleteOldPhotoFromCloud()
    //{
    //    if (!string.IsNullOrWhiteSpace(OldImage))
    //    {
    //        string filename = Path.GetFileName(new Uri(OldImage).LocalPath);
    //        await _blobService.DeleteBlobAsync(filename, collectionBlobContainerName);
    //    }
    //}

    //private void ToggleEditPhotoRequestStatus()
    //{
    //    editPhotoRequested = !editPhotoRequested;
    //}

    //private async Task SubmitEditPhoto()
    //{
    //    if (!string.IsNullOrWhiteSpace(UploadedFileName))
    //    {
    //        if (UploadedFileName != CurrentItem!.ImageLink)
    //        {
    //            await ChangeItemImage();
    //        }

    //        UpdateItemPhoto();
    //    }

    //    editPhotoRequested = !editPhotoRequested;
    //    InitializeData();
    //}

    //private void UpdateItemPhoto()
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    var tmp = context.Items.First(x => x.Id == CurrentItem!.Id);
    //    tmp.ImageLink = ItemModel!.ImageLink;
    //    context.SaveChanges();
    //}













    private void ResetCollectionChangeRequestValidStatus()
    {
        collectionChangeRequestValid = false;
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
        GetConfigurationData();
        SetUpFileTransferManager();
        InitializeData();
    }

    private void InitializeData()
    {
        FetchCollectionsDataFromDataSource();
        GetThemes();
        ItemModel = new();
        CollectionModel = new();
        if (collection is not null)
        {
            CollectionModel = new()
            {
                Id = collection!.Id,
                Name = collection.Name,
                Description = collection.Description,
                ThemeID = collection.ThemeID,
                ImageLink = collection.ImageLink,
            };
        }

        ThemeNameChoosen = Themes.First(x => x.Id == CollectionModel.ThemeID).Name;
    }

    private void ToggleNewItemRequestStatus()
    {
        TempImg = string.Empty;
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
        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            _fileTransferManager.DeleteFileFromDisk(UploadedFileName!);
        }

        //ItemModel!.ImageLink = TempImg;

        ItemModel!.CollectionId = collection!.Id;
        CreateNewItem();
        newItemRequested = false;
    }

    private void SubmitEditCollection()
    {
        if (ValidateCollectionModel())
        {
            UpdateCollection();
            editItemRequested = !editItemRequested;
            InitializeData();
        }
    }

    private bool ValidateCollectionModel()
    {
        if (CollectionModel is not null)
        {
            if (string.IsNullOrWhiteSpace(CollectionModel.Name) && CollectionModel.Name != collection!.Name)
            {
                CollectionModel.Name = collection!.Name;
            }

            if (string.IsNullOrWhiteSpace(CollectionModel.Description) && CollectionModel.Description != collection!.Description)
            {
                CollectionModel.Description = collection!.Description;
            }

            if (string.IsNullOrWhiteSpace(CollectionModel.ImageLink) && CollectionModel.ImageLink != collection!.ImageLink)
            {
                CollectionModel.ImageLink = collection!.ImageLink;
            }
            else
            {
                CollectionModel.ImageLink = TempImg;
            }

            CollectionModel.Id = collection!.Id;

            return true;
        }

        return false;
    }

    private string CreateMarkdown(string input)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
        return Markdown.ToHtml(input, pipeline);
    }

    private void UpdateCollection()
    {
        using var context = _contextFactory.CreateDbContext();
        var tmp = context.Collections.First(a => a.Id == collection!.Id);
        if (CollectionModel!.Name != collection!.Name)
        {
            tmp.Name = CollectionModel!.Name;
        }

        if (CollectionModel.Description != collection.Description)
        {
            tmp.Description = CreateMarkdown(CollectionModel.Description);
        }

        var t = Themes.First(x => x.Name == ThemeNameChoosen).Id;

        if (t != collection.ThemeID)
        {
            tmp.ThemeID = t;
        }

        if (CollectionModel.ImageLink != collection.ImageLink)
        {
            tmp.ImageLink = TempImg;
        }

        context.SaveChanges();
    }

    private void SubmitDeleteCollection()
    {
        DeleteCollection();
        _navigationManager.NavigateTo($"/collections");
    }

    private void DeleteCollection()
    {
        using var adc = _contextFactory.CreateDbContext();
        Collection temp = adc.Collections.Where(x => x.Id == collection!.Id).First();
        adc.Collections.Remove(temp);
        adc.SaveChanges();
    }

    private void FetchCollectionsDataFromDataSource()
    {
        itemsBunch = [];
        var t = GetCollectionsFromDataSource().ToList();
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