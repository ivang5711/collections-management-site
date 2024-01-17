using Collections.Data;
using Collections.Models;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;

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
    public string? TagToDelete { get; set; }
    private List<Item>? ItemsBunch { get; set; }
    public string? UploadedFileName { get; set; }
    private ApplicationUser? ThisUser { get; set; }
    public List<Tag> Tags { get; set; } = [];
    public string Error { get; set; } = string.Empty;
    private List<Comment> Comments { get; set; } = [];
    public string TempImg { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    private string OldImage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        GetAuthenticationState();
        itemBlobContainerName = _configuration.GetValue<string>("ItemBlobContainerName") ?? string.Empty;
        blobTempDirectoryPath = _configuration.GetValue<string>("BlobTempDirectoryPath") ?? string.Empty;
        blobTempDirectory = _configuration.GetValue<string>("BlobTempDirectory") ?? string.Empty;
        CreateData();
        UploadedFileName = null;
        CommentText = null;
        if (ItemsBunch is not null)
        {
            CurrentItem = ItemsBunch.First(x => x.Id == Id);
            Comments = CurrentItem.Comments.OrderByDescending(x => x.CreationDateTime).ToList();
            LikeCount = CurrentItem.Likes.Count;
            collection = CurrentItem.Collection;
            ItemModel = new()
            {
                Id = CurrentItem.Id,
                Name = CurrentItem.Name,
                ImageLink = CurrentItem.ImageLink,
                CreationDateTime = CurrentItem.CreationDateTime,
            };

            TempImg = CurrentItem.ImageLink!;
            OldImage = CurrentItem.ImageLink!;
        }
    }

    public async Task UploadFile(InputFileChangeEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            var basePath = Directory.GetCurrentDirectory();
            string path = Path.Combine(basePath, blobTempDirectoryPath, UploadedFileName);
            File.Delete(path);
        }

        Error = string.Empty;
        var file = e.File;

        if (file is not null)
        {
            FileName = file.Name;
            if (file.Size > maxFileSize)
            {
                Error = $"File is too big! The file size is limited to {maxFileSize}";
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
                Error = $"File Error: {exception}";
            }
        }
    }

    private async Task ChangeItemImage()
    {
        string basePath = Directory.GetCurrentDirectory();
        string path = Path.Combine(basePath, blobTempDirectoryPath, UploadedFileName!);
        string randomFileName = Path.GetRandomFileName();
        string fileExtension = Path.GetExtension(UploadedFileName!);
        string newFileName = Path.ChangeExtension(randomFileName, fileExtension);
        await _blobService.UploadFileBlobAsync(path, newFileName, itemBlobContainerName);
        ItemModel!.ImageLink = _blobService.GetBlobUrl(newFileName, itemBlobContainerName);
        
        if (!string.IsNullOrWhiteSpace(OldImage))
        {
            string filename = Path.GetFileName(new Uri(OldImage).LocalPath);
            await _blobService.DeleteBlobAsync(filename, itemBlobContainerName);
        }

        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            File.Delete(path);
        }
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

    private void OnRegisterSubmit()
    {
        _navigationManager.NavigateTo("/Account/Register");
    }

    private void OnLoginSubmit()
    {
        _navigationManager.NavigateTo("/Account/Login");
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

    private void SubmitDeleteItem()
    {
        DeleteItem();
        _navigationManager.NavigateTo($"/collection-details/{CollectionId}");
    }

    private void UpdateItemPhoto()
    {
        using var context = _contextFactory.CreateDbContext();
        var tmp = context.Items.First(x => x.Id == CurrentItem!.Id);
        tmp.ImageLink = ItemModel!.ImageLink;
        context.SaveChanges();
    }

    private void DeleteItem()
    {
        using var adc = _contextFactory.CreateDbContext();
        adc.Items.Remove(adc.Items.Where(x => x.Id == CurrentItem!.Id).First());
        adc.SaveChanges();
    }

    private void ToggleEditItemRequestStatus()
    {
        if (CurrentItem is not null)
        {
            ItemModel = new()
            {
                Id = CurrentItem.Id,
                Name = CurrentItem.Name,
                ImageLink = CurrentItem.ImageLink,
                CreationDateTime = CurrentItem.CreationDateTime,
            };
        }

        editItemRequested = !editItemRequested;
    }

    private void ToggleDeleteItemRequestStatus()
    {
        deleteItemRequested = !deleteItemRequested;
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
        StateHasChanged();
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
        var u = adc.Users.First(u => u.Id == ThisUser!.Id);
        var x = adc.Items.First(x => x.Id == CurrentItem!.Id);
        var temp = new Comment()
        {
            ItemId = x.Id,
            ApplicationUserId = u.Id,
            Text = CommentText!,
            CreationDateTime = DateTime.UtcNow
        };
        adc.Comments.Add(temp);
        adc.SaveChanges();
    }

    private void IncrementLike()
    {
        if (!CurrentItem!.Likes.Where(x => x.ApplicationUserId == ThisUser!.Id)!.Any())
        {
            using var adc = _contextFactory.CreateDbContext();
            var a = adc.Users.First(x => x.Id == ThisUser!.Id);
            var b = adc.Items.First(x => x.Id == CurrentItem.Id);
            adc.Likes.Add(new Like()
            {
                ApplicationUserId = a.Id,
                ItemId = b.Id
            });
            adc.SaveChanges();
        }
        else
        {
            using var adc = _contextFactory.CreateDbContext();
            var temp = adc.Likes.Where(x => x.ApplicationUserId == ThisUser!.Id).First(x => x.ItemId == CurrentItem.Id);
            adc.Likes.Remove(temp);
            adc.SaveChanges();
        }

        InitializeData();
    }

    private void CreateData()
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
        string res = string.Empty;
        using var adc = _contextFactory.CreateDbContext();
        res = adc.Users.First(x => x.Id == collection!.ApplicationUserId).FullName;

        return res;
    }

    private void GetAuthenticationState()
    {
        AuthenticationState authenticationState = Task.Run(() =>
                    _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        ThisUser = Task.Run(() =>
            _userManager.GetUserAsync(authenticationState.User)).Result;
    }

    private async Task Focus()
    {
        await JsRuntime.InvokeVoidAsync("focusOnElement", inputToFocus);
    }
}