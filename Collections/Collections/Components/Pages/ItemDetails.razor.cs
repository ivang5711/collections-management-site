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
    public int LikeCount { get; set; } = 999;

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
        var t = adc.Items
        .Include(e => e.Tags)
        .Include(e => e.Likes)
        .Include(e => e.Comments)
        .Include (e => e.Collection)
        .ToList();
        return t;
    }
}