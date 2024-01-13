using Collections.Data;
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
    private bool collectionChangeRequestValid = true;
    public Collection? CollectionModel { get; set; }
    private bool newItemRequested = false;
    private Collection? collection;

    private void ResetCollectionChangeRequestValidStatus()
    {
        collectionChangeRequestValid = false;
    }

    private void SubmitEditCollection()
    {
        Console.WriteLine("Edit collection submitted!");
        //if (ValidateCollectionModel())
        //{
        //    UpdateCollection();
        //    editItemRequested = !editItemRequested;
        //    InitializeData();
        //}
    }


    private void ToggleEditCollectionRequestStatus()
    {
        editItemRequested = !editItemRequested;
    }

    private void ToggleDeleteCollectionRequestStatus()
    {
        deleteItemRequested = !deleteItemRequested;
    }

    protected override async Task OnInitializedAsync()
    {
        _itemsBunch = CreateData2();
        _itemDetails = _itemsBunch.First(x => x.Id == Id);
        _comments = _itemDetails.Comments;
        LikeCount = _itemDetails.Likes.Count;
    }

    private void IncrementLike()
    {
        LikeCount++;
    }

    private List<Item> CreateData2()
    {
        List<Item> t;
        using (var adc = _contextFactory.CreateDbContext())
        {
            t =
            [
                .. adc.Items
                            .Include(e => e.Tags)
                            .Include(e => e.Likes)
                            .Include(e => e.Comments)
                            .Include(e => e.Collection)
,
            ];
        }
        return t;
    }

    private string GetAuthor(int collectionId)
    {
        string res = string.Empty;
        using (var adc = _contextFactory.CreateDbContext())
        {
            var temp = adc.Collections.First(x => x.Id == collectionId);
            res = adc.Users.First(x => x.Id == temp.ApplicationUserId).FullName;
        }

        return res;
    }
}