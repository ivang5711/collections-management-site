using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Collections.Components.Pages;

public partial class ItemDetails
{
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public int CollectionId { get; set; }

    private Item? _itemDetails;
    private List<Item>? _itemsBunch;
    private List<Comment> _comments = [];
    private bool editItemRequested = false;
    private bool deleteItemRequested = false;
    private ApplicationUser? ThisUser;

    public int LikeCount { get; set; } = 999;
    public Item? ItemModel { get; set; }
    private Collection? collection;

    private void SubmitEditItem()
    {
        if (ValidateItemModel())
        {
            UpdateItem();
            InitializeData();
            editItemRequested = !editItemRequested;
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

    private ElementReference InputToFocus;

    public string? CommentText { get; set; }

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