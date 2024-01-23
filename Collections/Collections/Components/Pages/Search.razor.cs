using Collections.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Collections.Components.Pages;

public partial class Search
{
    private int totalCountUnique;
    private List<Collection> Collections { get; set; } = [];
    private List<Item> Items { get; set; } = [];
    public string? SearchQuery { get; set; }
    public List<Collection> CollectionNameSearchResults { get; set; } = [];
    public List<Collection> CollectionDescriptionSearchResults { get; set; } = [];
    public List<Collection> ThemeSearchResults { get; set; } = [];
    public List<Item> ItemNameSearchResults { get; set; } = [];
    public List<Item> ItemCommentsSearchResults { get; set; } = [];
    public List<Item> TagSearchResults { get; set; } = [];
    public List<Item> ItemStringFieldSearchResults { get; set; } = [];
    public List<Item> ItemTextFieldSearchResults { get; set; } = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
        }
    }

    private async Task LoadStateAsync()
    {
        var result = await GetLSData("searchText");

        SearchQuery = !string.IsNullOrWhiteSpace(result) ? result : string.Empty;
        var test = SearchQuery;
        Console.WriteLine("Test:" + test);
        StateHasChanged();
        await DeleteLSData("searchText");

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            SubmitSearch();
        }
    }

    private async Task<string> GetLSData(string key)
    {
        var response = await JSRuntime.InvokeAsync<string>("getLocalStoreData", key);
        return response;
    }

    private async Task DeleteLSData(string key)
    {
        await JSRuntime.InvokeVoidAsync("removeLocalStoreData", key);
    }

    private List<Collection> SearchInCollectionName(string query)
    {
        using var adc = _contextFactory.CreateDbContext();
        var results = from p in adc.Collections
                      where EF.Functions.FreeText(p.Name, query)
                      select p;

        return [.. results];
    }

    private List<Collection> SearchInCollectionDescription(string query)
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var results = from p in adc.Collections
                          where EF.Functions.FreeText(p.Description, query)
                          select p;
            return [.. results];
        }
    }

    private List<Collection> SearchInCollectionTheme(string query)
    {
        IQueryable<Theme> results;
        using (var adc = _contextFactory.CreateDbContext())
        {
            results = from p in adc.Themes
                      where EF.Functions.FreeText(p.Name, query)
                      select p;

            List<Collection> col = [];
            col.AddRange(from t in results.ToList()
                         where adc.Collections.Any(x => x.ThemeID == t.Id)
                         select adc.Collections.First(x => x.ThemeID == t.Id));
            return col;
        }
    }

    private List<Item> SearchInItemName(string query)
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var results = from p in adc.Items
                          where EF.Functions.FreeText(p.Name, query)
                          select p;
            return [.. results];
        }
    }

    private List<Item> SearchInItemComment(string query)
    {
        IQueryable<Comment> results;
        using (var adc = _contextFactory.CreateDbContext())
        {
            results = from p in adc.Comments
                      where EF.Functions.FreeText(p.Text, query)
                      select p;

            List<Item> col = [];
            col.AddRange(from t in results.ToList()
                         where adc.Items.Any(x => x.Id == t.ItemId)
                         select adc.Items.First(x => x.Id == t.ItemId));
            return col;
        }
    }

    private List<Item> SearchInItemTags(string query)
    {
        IQueryable<Tag> results;
        using (var adc = _contextFactory.CreateDbContext())
        {
            results = from p in adc.Tags
                      where EF.Functions.FreeText(p.Name, query)
                      select p;

            List<Item> col = [];
            col.AddRange(from t in results.ToList()
                         where adc.Items.Any(x => x.Tags.Contains(t))
                         select adc.Items.First(x => x.Tags.Contains(t)));
            return col;
        }
    }

    private List<Item> SearchInItemStringField(string query)
    {
        IQueryable<StringField> results;
        using (var adc = _contextFactory.CreateDbContext())
        {
            results = from p in adc.StringFields
                      where EF.Functions.FreeText(p.Value, query)
                      select p;

            List<Item> col = [];
            col.AddRange(from t in results.ToList()
                         where adc.Items.Any(x => x.StringFields.Contains(t))
                         select adc.Items.First(x => x.StringFields.Contains(t)));
            return col;
        }
    }

    private List<Item> SearchInItemTextField(string query)
    {
        IQueryable<TextField> results;
        using (var adc = _contextFactory.CreateDbContext())
        {
            results = from p in adc.TextFields
                      where EF.Functions.FreeText(p.Value, query)
                      select p;

            List<Item> col = [];
            col.AddRange(from t in results.ToList()
                         where adc.Items.Any(x => x.TextFields.Contains(t))
                         select adc.Items.First(x => x.TextFields.Contains(t)));
            return col;
        }
    }

    private void SubmitSearch()
    {
        CollectionNameSearchResults = SearchInCollectionName(SearchQuery!);
        CollectionDescriptionSearchResults = SearchInCollectionDescription(SearchQuery!);
        ThemeSearchResults = SearchInCollectionTheme(SearchQuery!);

        ItemNameSearchResults = SearchInItemName(SearchQuery!);
        ItemCommentsSearchResults = SearchInItemComment(SearchQuery!);
        TagSearchResults = SearchInItemTags(SearchQuery!);
        ItemStringFieldSearchResults = SearchInItemStringField(SearchQuery!);
        ItemTextFieldSearchResults = SearchInItemTextField(SearchQuery!);

        List<Collection> collectionsFound = [];
        collectionsFound.AddRange(CollectionNameSearchResults);
        collectionsFound.AddRange(CollectionDescriptionSearchResults);
        collectionsFound.AddRange(ThemeSearchResults);
        Collections = collectionsFound
                .GroupBy(y => y.Id)
                .Select(y => y.First()).ToList();

        List<Item> itemsFound = [];
        itemsFound.AddRange(ItemNameSearchResults);
        itemsFound.AddRange(ItemCommentsSearchResults);
        itemsFound.AddRange(TagSearchResults);
        itemsFound.AddRange(ItemStringFieldSearchResults);
        itemsFound.AddRange(ItemTextFieldSearchResults);
        Items = itemsFound
        .GroupBy(y => y.Id)
        .Select(y => y.First()).ToList();

        totalCountUnique = Collections.Count + Items.Count;
        InvokeAsync(StateHasChanged);
    }
}