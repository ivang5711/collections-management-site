using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Collections.Components.Pages;

public partial class ItemDetails
{
    private Collection? collection;
    private ElementReference inputToFocus;
    private bool editItemRequested = false;
    private bool editPhotoRequested = false;
    private bool deleteItemRequested = false;
    private const int maxFileSize = 1024 * 1024 * 5;
    private string blobTempDirectory = string.Empty;
    private string itemBlobContainerName = string.Empty;
    private string blobTempDirectoryPath = string.Empty;

    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public int CollectionId { get; set; }

    public int LikeCount { get; set; }
    public string? NewTag { get; set; }
    public Item? ItemModel { get; set; }
    private Item? CurrentItem { get; set; }
    public string? CommentText { get; set; }
    private List<Item>? ItemsBunch { get; set; }
    public string? UploadedFileName { get; set; }
    private ApplicationUser? ThisUser { get; set; }
    public List<Tag> Tags { get; set; } = [];
    public string FileError { get; set; } = string.Empty;
    private List<Comment> Comments { get; set; } = [];
    public string TempImg { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    private string OldImage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await Task.Run(() => InitializeData());
    }

    private void InitializeData()
    {
        GetAuthenticationState();
        GetConfigurationData();
        GetItemData();
        SetUpItem();
    }

    private void SetUpItem()
    {
        UploadedFileName = null;
        CommentText = null;
        if (ItemsBunch is not null)
        {
            CurrentItem = ItemsBunch.First(x => x.Id == Id);
            SetItemState();
            CreateItemModel();
        }
    }

    private void SetItemState()
    {
        Comments = [.. CurrentItem!.Comments
            .OrderByDescending(x => x.CreationDateTime)];
        LikeCount = CurrentItem.Likes.Count;
        collection = CurrentItem.Collection;
        TempImg = CurrentItem.ImageLink!;
        OldImage = CurrentItem.ImageLink!;
    }

    private void GetConfigurationData()
    {
        itemBlobContainerName = _configuration.GetValue<string>("ItemBlobContainerName") ?? string.Empty;
        blobTempDirectoryPath = _configuration.GetValue<string>("BlobTempDirectoryPath") ?? string.Empty;
        blobTempDirectory = _configuration.GetValue<string>("BlobTempDirectory") ?? string.Empty;
    }

    private void GetItemData()
    {
        using var adc = _contextFactory.CreateDbContext();
        ItemsBunch =
        [
            .. adc.Items
                        .Include(e => e.Tags)
                        .Include(e => e.Likes)
                        .Include(e => e.Comments)
                        .ThenInclude(e => e.ApplicationUser)
                        .Include(e => e.Collection)
        ];
        Tags = [.. adc.Tags.OrderByDescending(x => x.Items.Count)];
    }

    private string GetAuthor()
    {
        using var adc = _contextFactory.CreateDbContext();
        return adc.Users.First(x => x.Id == collection!.ApplicationUserId).FullName;
    }

    private void CreateItemModel()
    {
        ItemModel = new()
        {
            Id = CurrentItem!.Id,
            Name = CurrentItem.Name,
            ImageLink = CurrentItem.ImageLink,
            CreationDateTime = CurrentItem.CreationDateTime,
        };
    }

    public async Task UploadFile(InputFileChangeEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            DeletePreviousPhoto();
        }

        FileError = string.Empty;
        var file = e.File;
        if (file is not null)
        {
            FileName = file.Name;
            if (file.Size > maxFileSize)
            {
                FileError = $"File is too big! The file size is limited to {maxFileSize}";
                return;
            }

            try
            {
                var randomFileName = Path.GetRandomFileName();
                var fileExtension = Path.GetExtension(file.Name);
                var newFileName = Path.ChangeExtension(randomFileName, fileExtension);
                var basePath = Directory.GetCurrentDirectory();
                string path = Path.Combine(basePath, blobTempDirectoryPath, newFileName);
                Directory.CreateDirectory(Path.Combine(basePath, blobTempDirectoryPath));
                using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
                UploadedFileName = newFileName;
                TempImg = Path.Combine(blobTempDirectory, newFileName);
                StateHasChanged();
            }
            catch (Exception exception)
            {
                FileError = $"File Error: {exception}";
            }
        }
    }

    private void DeletePreviousPhoto()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(),
            blobTempDirectoryPath, UploadedFileName!);
        File.Delete(path);
    }

    private async Task ChangeItemImage()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(),
            blobTempDirectoryPath, UploadedFileName!);
        await UploadFileToCloud(path);
        AssignNewImageToItem();
        await CleanOldPhotos(path);
    }

    private async Task UploadFileToCloud(string path)
    {
        await _blobService.UploadFileBlobAsync(path, UploadedFileName!,
            itemBlobContainerName);
    }

    private void AssignNewImageToItem()
    {
        ItemModel!.ImageLink = _blobService.GetBlobUrl(UploadedFileName!,
            itemBlobContainerName);
    }

    private async Task CleanOldPhotos(string path)
    {
        await DeleteOldPhotoFromCloud();
        File.Delete(path);
    }

    private async Task DeleteOldPhotoFromCloud()
    {
        if (!string.IsNullOrWhiteSpace(OldImage))
        {
            string filename = Path.GetFileName(new Uri(OldImage).LocalPath);
            await _blobService.DeleteBlobAsync(filename, itemBlobContainerName);
        }
    }

    private void ToggleEditPhotoRequestStatus()
    {
        editPhotoRequested = !editPhotoRequested;
    }

    private async Task SubmitEditPhoto()
    {
        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            if (UploadedFileName != CurrentItem!.ImageLink)
            {
                await ChangeItemImage();
            }

            UpdateItemPhoto();
        }

        editPhotoRequested = !editPhotoRequested;
        InitializeData();
    }

    private void UpdateItemPhoto()
    {
        using var context = _contextFactory.CreateDbContext();
        var tmp = context.Items.First(x => x.Id == CurrentItem!.Id);
        tmp.ImageLink = ItemModel!.ImageLink;
        context.SaveChanges();
    }

    private void SubmitAddTag()
    {
        if (!string.IsNullOrWhiteSpace(NewTag))
        {
            if (CheckTagPresence())
            {
                NewTag = null;
                return;
            }

            CreateTag();
            AddTag();
            InitializeData();
        }

        NewTag = null;
    }

    private bool CheckTagPresence()
    {
        return CurrentItem!.Tags.Exists(x => x.Name == NewTag);
    }

    private void SubmitRemoveTag(int id)
    {
        RemoveTag(id);
        InitializeData();
        StateHasChanged();
    }

    private void RemoveTag(int id)
    {
        using var adc = _contextFactory.CreateDbContext();
        Tag temp = adc.Tags.Where(x => x.Id == id).First();
        adc.Tags.Remove(temp);
        adc.SaveChanges();
    }

    private void AddTag()
    {
        using var adc = _contextFactory.CreateDbContext();
        var a = adc.Items.First(x => x.Id == Id);
        var b = adc.Tags.First(x => x.Name == NewTag);
        a.Tags.Add(b);
        adc.SaveChanges();
    }

    private void CreateTag()
    {
        using var adc = _contextFactory.CreateDbContext();
        adc.Tags.Add(new Tag() { Name = NewTag! });
        adc.SaveChanges();
    }

    private void ToggleEditItemRequestStatus()
    {
        if (CurrentItem is not null)
        {
            CreateItemModel();
        }

        editItemRequested = !editItemRequested;
    }

    private void SubmitEditItem()
    {
        if (ValidateItemModel())
        {
            if (!string.IsNullOrWhiteSpace(ItemModel!.Name))
            {
                UpdateItem();
            }

            editItemRequested = !editItemRequested;
            StateHasChanged();
        }
    }

    private void UpdateItem()
    {
        using var context = _contextFactory.CreateDbContext();
        var tmp = context.Items.First(x => x.Id == CurrentItem!.Id);
        tmp.Name = ItemModel!.Name;
        context.SaveChanges();
    }

    private bool ValidateItemModel()
    {
        if (ItemModel is not null)
        {
            if (!string.IsNullOrWhiteSpace(ItemModel.Name))
            {
                CurrentItem!.Name = ItemModel.Name;
            }

            return true;
        }

        return false;
    }

    private void ToggleDeleteItemRequestStatus()
    {
        deleteItemRequested = !deleteItemRequested;
    }

    private void SubmitDeleteItem()
    {
        DeleteItem();
        _navigationManager.NavigateTo($"/collection-details/{CollectionId}");
    }

    private void DeleteItem()
    {
        using var adc = _contextFactory.CreateDbContext();
        adc.Items.Remove(adc.Items.Where(x => x.Id == CurrentItem!.Id).First());
        adc.SaveChanges();
    }

    private void SubmitComment()
    {
        if (!string.IsNullOrWhiteSpace(CommentText))
        {
            AddComment();
        }

        InitializeData();
    }

    private void AddComment()
    {
        using var adc = _contextFactory.CreateDbContext();
        var user = adc.Users.First(u => u.Id == ThisUser!.Id);
        var item = adc.Items.First(x => x.Id == CurrentItem!.Id);
        adc.Comments.Add(new Comment()
        {
            ItemId = item.Id,
            ApplicationUserId = user.Id,
            Text = CommentText!,
            CreationDateTime = DateTime.UtcNow
        });
        adc.SaveChanges();
    }

    private void ProcessLikeRequest()
    {
        if (!CurrentItem!.Likes
            .Where(x => x.ApplicationUserId == ThisUser!.Id)!.Any())
        {
            IncrementLike();
        }
        else
        {
            DecrementLike();
        }

        InitializeData();
    }

    private void DecrementLike()
    {
        using var adc = _contextFactory.CreateDbContext();
        var temp = adc.Likes.Where(x => x.ApplicationUserId == ThisUser!.Id)
            .First(x => x.ItemId == CurrentItem.Id);
        adc.Likes.Remove(temp);
        adc.SaveChanges();
    }

    private void IncrementLike()
    {
        using var adc = _contextFactory.CreateDbContext();
        var user = adc.Users.First(x => x.Id == ThisUser!.Id);
        var item = adc.Items.First(x => x.Id == CurrentItem!.Id);
        adc.Likes.Add(new Like()
        {
            ApplicationUserId = user.Id,
            ItemId = item.Id
        });
        adc.SaveChanges();
    }

    private void OnRegisterSubmit()
    {
        _navigationManager.NavigateTo("/Account/Register");
    }

    private void OnLoginSubmit()
    {
        _navigationManager.NavigateTo("/Account/Login");
    }

    private void GetAuthenticationState()
    {
        AuthenticationState authenticationState = Task.Run(() =>
                    _AuthenticationStateProvider
                    .GetAuthenticationStateAsync()).Result;
        ThisUser = Task.Run(() =>
            _userManager.GetUserAsync(authenticationState.User)).Result;
    }

    private async Task Focus()
    {
        await JsRuntime.InvokeVoidAsync("focusOnElement", inputToFocus);
    }
}