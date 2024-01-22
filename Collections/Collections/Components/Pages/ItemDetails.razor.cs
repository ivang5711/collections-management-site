using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Collections.Components.Pages;

public partial class ItemDetails
{
    private Collection? collection;
    private ElementReference inputToFocus;
    private int maxFileSize;
    private bool editItemRequested = false;
    private bool editPhotoRequested = false;
    private bool deleteItemRequested = false;
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
    public string? UploadedFileName { get; set; }
    private ApplicationUser? ThisUser { get; set; }
    public List<Tag> Tags { get; set; } = [];
    private List<Comment> Comments { get; set; } = [];
    public string TempImg { get; set; } = string.Empty;
    private string OldImage { get; set; } = string.Empty;
    public string FileError { get; set; } = string.Empty;

    private HubConnection? hubConnection;

    private async Task Send()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("UpdateComments");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
        .WithUrl(_navigationManager.ToAbsoluteUri("/commentshub"))
        .WithAutomaticReconnect()
        .Build();

        hubConnection.On("ReceiveUpdateComments", () =>
        {
            InitializeData();
            InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
        await Task.Run(() => InitializeData());
    }

    private void InitializeData()
    {
        GetAuthenticationState();
        GetConfigurationData();
        SetUpFileTransferManager();
        GetItemData();
        SetUpItem();
    }

    private void SetUpFileTransferManager()
    {
        _fileTransferManager.SetUpMaxFileSize(maxFileSize);
        _fileTransferManager.SetUpWorkingDirectory(blobTempDirectoryPath);
    }

    private void SetUpItem()
    {
        UploadedFileName = null;
        CommentText = null;
        SetItemState();
        CreateItemModel();
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
        itemBlobContainerName = _configuration
            .GetValue<string>("ItemBlobContainerName") ?? string.Empty;
        blobTempDirectoryPath = _configuration
            .GetValue<string>("BlobTempDirectoryPath") ?? string.Empty;
        blobTempDirectory = _configuration
            .GetValue<string>("BlobTempDirectory") ?? string.Empty;
        maxFileSize = _configuration.GetValue<int>("MaxFileSize");
    }

    private void GetItemData()
    {
        using var adc = _contextFactory.CreateDbContext();
        Tags = [.. adc.Tags.OrderByDescending(x => x.Items.Count)];
        CurrentItem = adc.Items
                        .Include(e => e.Tags)
                        .Include(e => e.Likes)
                        .Include(e => e.NumericalFields)
                        .Include(e => e.StringFields)
                        .Include(e => e.TextFields)
                        .Include(e => e.LogicalFields)
                        .Include(e => e.DateFields)
                        .Include(e => e.Comments)
                        .ThenInclude(e => e.ApplicationUser)
                        .Include(e => e.Collection)
                        .First(x => x.Id == Id);
    }

    private string GetAuthor()
    {
        using var adc = _contextFactory.CreateDbContext();
        return adc.Users
            .First(x => x.Id == collection!.ApplicationUserId).FullName;
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

        ItemModel.NumericalFields.AddRange(CurrentItem.NumericalFields);
        ItemModel.StringFields.AddRange(CurrentItem.StringFields);
        ItemModel.TextFields.AddRange(CurrentItem.TextFields);
        ItemModel.LogicalFields.AddRange(CurrentItem.LogicalFields);
        ItemModel.DateFields.AddRange(CurrentItem.DateFields);
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

    private async Task ChangeItemImage()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(),
            blobTempDirectoryPath, UploadedFileName!);
        await UploadFileToCloud(path);
        AssignNewImageToItem();
        await DeleteOldPhotoFromCloud();
        _fileTransferManager.DeleteFileFromDisk(UploadedFileName!);
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
        var numericalFields = context.NumericalFields.Where(x => x.Items.Contains(tmp)).ToList();

        foreach (var item in numericalFields)
        {
            item.Name = ItemModel!.NumericalFields.First(x => x.Id == item.Id).Name;
            item.Value = ItemModel!.NumericalFields.First(x => x.Id == item.Id).Value;
        }

        var stringFields = context.StringFields.Where(x => x.Items.Contains(tmp)).ToList();

        foreach (var item in stringFields)
        {
            item.Name = ItemModel!.StringFields.First(x => x.Id == item.Id).Name;
            item.Value = ItemModel!.StringFields.First(x => x.Id == item.Id).Value;
        }

        var textFields = context.TextFields.Where(x => x.Items.Contains(tmp)).ToList();

        foreach (var item in textFields)
        {
            item.Name = ItemModel!.TextFields.First(x => x.Id == item.Id).Name;
            item.Value = ItemModel!.TextFields.First(x => x.Id == item.Id).Value;
        }

        var logicalFields = context.LogicalFields.Where(x => x.Items.Contains(tmp)).ToList();

        foreach (var item in logicalFields)
        {
            item.Name = ItemModel!.LogicalFields.First(x => x.Id == item.Id).Name;
            item.Value = ItemModel!.LogicalFields.First(x => x.Id == item.Id).Value;
        }

        var dateFields = context.DateFields.Where(x => x.Items.Contains(tmp)).ToList();

        foreach (var item in dateFields)
        {
            item.Name = ItemModel!.DateFields.First(x => x.Id == item.Id).Name;
            item.Value = ItemModel!.DateFields.First(x => x.Id == item.Id).Value;
        }

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

    private async Task SubmitComment()
    {
        if (!string.IsNullOrWhiteSpace(CommentText))
        {
            AddComment();
        }
        await Send();
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
            .First(x => x.ItemId == CurrentItem!.Id);
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