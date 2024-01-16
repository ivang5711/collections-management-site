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
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public int CollectionId { get; set; }

    public string TempImg { get; set; } = string.Empty;

    private Item? _itemDetails;
    private List<Item>? _itemsBunch;
    private List<Comment> _comments = [];
    private bool editItemRequested = false;
    private bool deleteItemRequested = false;
    private ApplicationUser? ThisUser;

    private string OldImage = string.Empty;
    public string? CommentText { get; set; }
    public List<Tag> Tags { get; set; }

    public string? NewTag { get; set; }

    public int LikeCount { get; set; } = 999;
    public Item? ItemModel { get; set; }
    private Collection? collection;
    private ElementReference InputToFocus;
    public string? UploadedFileName { get; set; }

    public string FileName { get; set; } = string.Empty;
    private const int maxFileSize = 1024 * 1024 * 5; // 5 MB
    public string Error { get; set; } = string.Empty;

    public async Task UploadFile(InputFileChangeEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            var basePath = Directory.GetCurrentDirectory();
            string path = Path.Combine(basePath, "wwwroot/bloobtempfolder", UploadedFileName);
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
                string path = Path.Combine(basePath, "wwwroot/bloobtempfolder", newFileName);
                Directory.CreateDirectory(Path.Combine(basePath, "wwwroot/bloobtempfolder"));
                using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
                UploadedFileName = newFileName;
                TempImg = Path.Combine("bloobtempfolder", newFileName);
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
        var basePath = Directory.GetCurrentDirectory();
        string path = Path.Combine(basePath, "wwwroot/bloobtempfolder", UploadedFileName!);

        var randomFileName = Path.GetRandomFileName();
        var fileExtension = Path.GetExtension(UploadedFileName);
        var newFileName = Path.ChangeExtension(randomFileName, fileExtension);

        await _blobService.UploadFileBlobAsync(path, newFileName, "items");

        var res2 = await _blobService.ListBlobsAsync("items");

        var res0 = _blobService.GetBlobUrl(newFileName, "items");

        ItemModel!.ImageLink = res0;

        if (!string.IsNullOrWhiteSpace(OldImage))
        {
            Uri uri = new(OldImage);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            await _blobService.DeleteBlobAsync(filename, "items");
        }

        if (!string.IsNullOrWhiteSpace(UploadedFileName))
        {
            var basePath2 = Directory.GetCurrentDirectory();
            string path2 = Path.Combine(basePath2, "wwwroot/bloobtempfolder", UploadedFileName);
            File.Delete(path2);
        }
    }

    private void SubmitAddTag()
    {
        Console.WriteLine($"Tag: {NewTag}");
        if (!string.IsNullOrWhiteSpace(NewTag))
        {
            AddTag();
            InitializeData();
        }

        NewTag = null;
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
        adc.Tags.Add(new Tag() { Name = NewTag! });
        adc.SaveChanges();
        var a = adc.Items.First(x => x.Id == Id);
        var b = adc.Tags.First(x => x.Name == NewTag);
        a.Tags.Add(b);
        adc.SaveChanges();
    }

    private async Task SubmitEditItem()
    {
        if (ValidateItemModel())
        {
            if (UploadedFileName != _itemDetails!.ImageLink)
            {
                await ChangeItemImage();
            }
            UpdateItem();
            editItemRequested = !editItemRequested;
            _navigationManager.NavigateTo($"/item-details/{CollectionId}/{Id}", true);
            StateHasChanged();
        }
    }

    private void SubmitAddComment()
    {
        using var adc = _contextFactory.CreateDbContext();
        var a = adc.Users.First(x => x.Id == ThisUser!.Id);
        var b = adc.Items.First(x => x.Id == Id);
        var temp = new Comment
        {
            ApplicationUserId = a.Id,
            ItemId = b.Id,
            Text = "Great product for sure!",
            CreationDateTime = DateTime.UtcNow
        };

        adc.Comments.Add(temp);
        adc.SaveChanges();
        InitializeData();
    }

    private void UpdateItem()
    {
        using var context = _contextFactory.CreateDbContext();
        var tmp = context.Items.First(x => x.Id == _itemDetails!.Id);
        tmp.Name = ItemModel!.Name;
        tmp.ImageLink = ItemModel.ImageLink;
        context.SaveChanges();
    }

    private bool ValidateItemModel()
    {
        if (ItemModel is not null)
        {
            if (!string.IsNullOrWhiteSpace(ItemModel.Name))
            {
                _itemDetails!.Name = ItemModel.Name;
            }

            if (!string.IsNullOrWhiteSpace(ItemModel.ImageLink))
            {
                _itemDetails!.ImageLink = ItemModel.ImageLink;
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

    private void DeleteItem()
    {
        using var adc = _contextFactory.CreateDbContext();
        Item temp = adc.Items.Where(x => x.Id == _itemDetails!.Id).First();
        adc.Items.Remove(temp);
        adc.SaveChanges();
    }

    private void ToggleEditItemRequestStatus()
    {
        if (_itemDetails is not null)
        {
            ItemModel = new()
            {
                Id = _itemDetails.Id,
                Name = _itemDetails.Name,
                ImageLink = _itemDetails.ImageLink,
                CreationDateTime = _itemDetails.CreationDateTime,
            };
        }

        editItemRequested = !editItemRequested;
    }

    private void ToggleDeleteItemRequestStatus()
    {
        deleteItemRequested = !deleteItemRequested;
    }

    protected override async Task OnInitializedAsync()
    {
        InitializeData();
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
        var x = adc.Items.First(x => x.Id == _itemDetails!.Id);
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

    private async Task Focus()
    {
        await JsRuntime.InvokeVoidAsync("focusOnElement", InputToFocus);
    }

    private void InitializeData()
    {
        GetAuthenticationState();
        CreateData();
        UploadedFileName = null;
        CommentText = null;
        if (_itemsBunch is not null)
        {
            _itemDetails = _itemsBunch.First(x => x.Id == Id);
            _comments = _itemDetails.Comments.OrderByDescending(x => x.CreationDateTime).ToList();
            LikeCount = _itemDetails.Likes.Count;
            collection = _itemDetails.Collection;
            ItemModel = new()
            {
                Id = _itemDetails.Id,
                Name = _itemDetails.Name,
                ImageLink = _itemDetails.ImageLink,
                CreationDateTime = _itemDetails.CreationDateTime,
            };

            TempImg = _itemDetails.ImageLink!;
            OldImage = _itemDetails.ImageLink!;
        }
    }

    private void IncrementLike()
    {
        if (!_itemDetails!.Likes.Where(x => x.ApplicationUserId == ThisUser!.Id)!.Any())
        {
            using var adc = _contextFactory.CreateDbContext();
            var a = adc.Users.First(x => x.Id == ThisUser!.Id);
            var b = adc.Items.First(x => x.Id == _itemDetails.Id);
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
            var temp = adc.Likes.Where(x => x.ApplicationUserId == ThisUser!.Id).First(x => x.ItemId == _itemDetails.Id);
            adc.Likes.Remove(temp);
            adc.SaveChanges();
        }

        InitializeData();
    }

    private void CreateData()
    {
        using var adc = _contextFactory.CreateDbContext();
        _itemsBunch =
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
}