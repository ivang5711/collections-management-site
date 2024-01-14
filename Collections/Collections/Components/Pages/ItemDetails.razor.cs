using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    private void InitializeData()
    {
        GetAuthenticationState();
        CreateData();
        if (_itemsBunch is not null)
        {
            _itemDetails = _itemsBunch.First(x => x.Id == Id);
            _comments = _itemDetails.Comments;
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
            Console.WriteLine("Like incremented by 1");
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
            Console.WriteLine("The like is already there");
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