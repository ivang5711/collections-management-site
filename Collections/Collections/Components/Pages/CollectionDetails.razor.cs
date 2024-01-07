using Collections.Components.TestData;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace Collections.Components.Pages;

public partial class CollectionDetails
{
    [Parameter]
    public int Id { get; set; }
    private Collection? collection;
    private List<Item>? itemsBunch;
    private readonly DataGenerator dg = new("en");
    private const int seed = 123;

    protected override async Task OnInitializedAsync()
    {
        CreateData();
        itemsBunch = await Task.Run(() => CreateData());
    }

    public List<Item> CreateData()
    {
        List<Collection> Collections = dg.GenerateCollection(5, seed);
        collection = Collections.First((x) => x.Id == Id);
        List<Item> temp = dg.GenerateItems(500, seed);
        List<Item> res = [];
        if (collection is not null)
        {
            res.AddRange(from Item item in temp
                         where collection.ItemsIds.Contains(item.Id)
                         select item);
        }

        return res;
    }
}