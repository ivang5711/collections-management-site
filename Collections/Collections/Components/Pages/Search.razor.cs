using Collections.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Collections.Components.Pages;

public partial class Search
{
    private int totalCountUnique;
    private List<Item> items = [];
    private List<Item> tagSearchResults = [];
    private List<Collection> collections = [];
    private string? searchQuery = string.Empty;
    private List<Item> itemNameSearchResults = [];
    private List<Collection> themeSearchResults = [];
    private List<Item> itemCommentsSearchResults = [];
    private List<Item> itemTextFieldSearchResults = [];
    private List<Item> itemStringFieldSearchResults = [];
    private List<Collection> collectionNameSearchResults = [];
    private List<Collection> collectionDescriptionSearchResults = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
        }
    }

    private async Task LoadStateAsync()
    {
        var result = await GetBrowserLocalStoreData("searchText");
        searchQuery = result;
        await DeleteBrowserLocalStoreData("searchText");
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            SubmitSearch();
        }
        else
        {
            searchQuery = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task<string> GetBrowserLocalStoreData(string key) 
    {
        return await JSRuntime
            .InvokeAsync<string>("getLocalStoreData", key);
    }

    private async Task DeleteBrowserLocalStoreData(string key)
    {
        await JSRuntime.InvokeVoidAsync("removeLocalStoreData", key);
    }

    private void SearchInCollectionName()
    {
        using var adc = _contextFactory.CreateDbContext();
        var results = adc.Collections
            .Where(a => EF.Functions.FreeText(a.Name, searchQuery!));
        collectionNameSearchResults = [.. results];
    }

    private void SearchInCollectionDescription()
    {
        using var adc = _contextFactory.CreateDbContext();
        var results = adc.Collections
            .Where(a => EF.Functions.FreeText(a.Description, searchQuery!));
        collectionDescriptionSearchResults = [.. results];
    }

    private void SearchInCollectionTheme()
    {
        using var adc = _contextFactory.CreateDbContext();
        List<Theme> results = [.. adc.Themes
            .Where(a => EF.Functions.FreeText(a.Name, searchQuery!))];
        foreach (var c in results)
        {
            themeSearchResults.AddRange(adc.Collections
                .Where(x => x.ThemeID == c.Id));
        }
    }

    private void SearchInItemName()
    {
        using var adc = _contextFactory.CreateDbContext();
        var temp = adc.Items
            .Where(a => EF.Functions.FreeText(a.Name, searchQuery!));
        itemNameSearchResults = [.. temp];
    }

    private void SearchInItemComment()
    {
        using var adc = _contextFactory.CreateDbContext();
        List<Comment> results = [.. adc.Comments
            .Where(a => EF.Functions.FreeText(a.Text, searchQuery!))];
        foreach (var c in results)
        {
            itemCommentsSearchResults.AddRange(adc.Items
                .Where(x => x.Id == c.ItemId));
        }
    }

    private void SearchInItemTags()
    {
        using var adc = _contextFactory.CreateDbContext();
        List<Tag> results = [.. adc.Tags
            .Where(a => EF.Functions.FreeText(a.Name, searchQuery!))];
        foreach (var c in results)
        {
            tagSearchResults.AddRange(adc.Items
                .Where(x => x.Tags.Contains(c)));
        }
    }

    private void SearchInItemStringField()
    {
        using var adc = _contextFactory.CreateDbContext();
        List<StringField> results = [.. adc.StringFields
            .Where(a => EF.Functions.FreeText(a.Value, searchQuery!))];
        foreach (var c in results)
        {
            itemStringFieldSearchResults.AddRange(adc.Items
                .Where(x => x.StringFields.Contains(c)));
        }
    }

    private void SearchInItemTextField()
    {
        using var adc = _contextFactory.CreateDbContext();
        List<TextField> results = [.. adc.TextFields
            .Where(a => EF.Functions.FreeText(a.Value, searchQuery!))];
        foreach (var c in results)
        {
            itemTextFieldSearchResults.AddRange(adc.Items
                .Where(x => x.TextFields.Contains(c)));
        }
    }

    private void SubmitSearch()
    {
        SearchInDataSource();
        PopulateSearchResults();
        RemoveDuplicatesFromSearchResults();
        SetTotalSearchResultsCount();
        InvokeAsync(StateHasChanged);
    }

    private void RemoveDuplicatesFromSearchResults()
    {
        RemoveCollectionDuplicates();
        RemoveItemDuplicates();
    }

    private void PopulateSearchResults()
    {
        PopulateCollections();
        PopulateItems();
    }

    private void SearchInDataSource()
    {
        SearchInCollectionRelatedFields();
    }

    private void SearchInCollectionRelatedFields()
    {
        SearchInCollectionName();
        SearchInCollectionDescription();
        SearchInCollectionTheme();
        SearchInItemRelatedFields();
    }

    private void SearchInItemRelatedFields()
    {
        SearchInItemName();
        SearchInItemComment();
        SearchInItemTags();
        SearchInItemStringField();
        SearchInItemTextField();
    }

    private void SetTotalSearchResultsCount()
    {
        totalCountUnique = collections.Count + items.Count;
    }

    private void PopulateItems()
    {
        items.AddRange(itemNameSearchResults);
        items.AddRange(itemCommentsSearchResults);
        items.AddRange(tagSearchResults);
        items.AddRange(itemStringFieldSearchResults);
        items.AddRange(itemTextFieldSearchResults);
    }

    private void PopulateCollections()
    {
        collections.AddRange(collectionNameSearchResults);
        collections.AddRange(collectionDescriptionSearchResults);
        collections.AddRange(themeSearchResults);
    }

    private void RemoveItemDuplicates()
    {
        items = items
            .GroupBy(y => y.Id)
            .Select(y => y.First())
            .ToList();
    }

    private void RemoveCollectionDuplicates()
    {
        collections = collections
                .GroupBy(y => y.Id)
                .Select(y => y.First())
                .ToList();
    }
}