using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Collections.Components.Pages;

public partial class CollectionDetails
{
    [Parameter]
    public int Id { get; set; }

    private Collection? collection;
    private List<Item>? itemsBunch;

    protected override async Task OnInitializedAsync()
    {
        itemsBunch = [];

        var t = CreateData2().ToList<Collection>();
        collection = t.First(x => x.Id == Id);
        itemsBunch.AddRange(collection.Items);
    }

    private List<Collection> CreateData2()
    {
        var t = adc.Collections
        .Include(e => e.Theme)
        .Include(e => e.Items)
        .ThenInclude(e => e.Tags)
        .Include(e => e.Items)
        .ThenInclude(e => e.Likes)
        .ToList();
        return t;
    }
}