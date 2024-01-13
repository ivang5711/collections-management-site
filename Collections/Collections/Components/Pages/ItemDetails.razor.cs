using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Collections.Components.Pages;

public partial class ItemDetails
{
    [Parameter]
    public int Id { get; set; }

    private Item? _itemDetails;
    private List<Item>? _itemsBunch;
    private List<Comment> _comments = [];
    private bool editItemRequested = false;
    private bool deleteItemRequested = false;
    public int LikeCount { get; set; } = 999;
    private string? TempImg { get; set; } = string.Empty;
    private bool itemChangeRequestValid = true;
    public Item? ItemModel { get; set; }
    private bool newItemRequested = false;
    private Collection? collection;

    private void ResetCollectionChangeRequestValidStatus()
    {
        itemChangeRequestValid = false;
    }

    private void SubmitEditItem()
    {
        Console.WriteLine("Edit Item submitted!");
        //if (ValidateCollectionModel())
        //{
        //    UpdateCollection();
        //    editItemRequested = !editItemRequested;
        //    InitializeData();
        //}
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
            TempImg = _itemDetails.ImageLink;
        }

        editItemRequested = !editItemRequested;
    }

    private void ToggleDeleteItemRequestStatus()
    {
        deleteItemRequested = !deleteItemRequested;
    }

    protected override async Task OnInitializedAsync()
    {
        CreateData();
        if (_itemsBunch is not null)
        {
            _itemDetails = _itemsBunch.First(x => x.Id == Id);
            _comments = _itemDetails.Comments;
            LikeCount = _itemDetails.Likes.Count;
            collection = _itemDetails.Collection;
            if (_itemDetails is not null)
            {
                ItemModel = new()
                {
                    Id = _itemDetails.Id,
                    Name = _itemDetails.Name,
                    ImageLink = _itemDetails.ImageLink,
                    CreationDateTime = _itemDetails.CreationDateTime,
                };
                TempImg = _itemDetails.ImageLink;
            }
        }
    }

    private void IncrementLike()
    {
        LikeCount++;
    }

    private void CreateData()
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            _itemsBunch =
            [
                .. adc.Items
                            .Include(e => e.Tags)
                            .Include(e => e.Likes)
                            .Include(e => e.Comments)
                            .Include(e => e.Collection)
            ];
        }
    }

    private string GetAuthor()
    {
        string res = string.Empty;
        using (var adc = _contextFactory.CreateDbContext())
        {
            res = adc.Users.First(x => x.Id == collection!.ApplicationUserId).FullName;
        }

        return res;
    }
}