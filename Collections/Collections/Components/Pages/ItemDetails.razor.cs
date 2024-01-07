using Collections.Components.TestData;
using Collections.Models;
using Microsoft.AspNetCore.Components;

namespace Collections.Components.Pages;
public partial class ItemDetails
{
    [Parameter]
    public int Id { get; set; }
    private Item? _itemDetails;
    private List<Item>? _itemsBunch;
    private List<Comment>? _comments = [];

    public Item CreateData()
    {
        DataGenerator dg = new("en");
        _itemsBunch = dg.GenerateItems(500, 123);
        var t = _itemsBunch.First((x) => x.Id == Id);
        List<Comment> temp = dg.GenerateComments(500, 123);
        foreach (var item in temp)
        {
            if (t.CommentsIds!.Contains(item.Id))
            {
                _comments!.Add(item);
            }
        }

        return _itemsBunch.First((x) => x.Id == Id);
    }

    protected override async Task OnInitializedAsync()
    {
        CreateData();
        _itemDetails = await Task.Run(() => CreateData());
    }
}
