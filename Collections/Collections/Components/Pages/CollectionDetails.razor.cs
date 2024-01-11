using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Collections.Components.Pages;

public partial class CollectionDetails
{
    [Parameter]
    public int Id { get; set; }

    private Collection? collection;
    private List<Item>? itemsBunch;
    private ApplicationUser? ThisUser;
    private bool newItemRequested = false;
    private const string roleBlocked = "Blocked";
    private const string loginPageURL = "/Account/Login";
    private string TempImg { get; set; } = string.Empty;

    [SupplyParameterFromForm]
    public ItemCandidate? Model { get; set; }

    public class ItemCandidate
    {
        public string? ImageLink { get; set; }

        [Required]
        public int CollectionId { get; set; }

        [Required]
        public string? Name { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthorizationLevel();
        itemsBunch = [];
        var t = CreateData2().ToList<Collection>();
        collection = t.First(x => x.Id == Id);
        itemsBunch.AddRange(collection.Items);
        Model ??= new();
    }

    private List<Collection> CreateData2()
    {
        List<Collection> t;
        using (var adc = _contextFactory.CreateDbContext())
        {
            t = adc.Collections
           .Include(e => e.Theme)
           .Include(e => e.Items)
           .ThenInclude(e => e.Tags)
           .Include(e => e.Items)
           .ThenInclude(e => e.Likes)
           .ToList();
        }
        return t;
    }

    private void CreateItem(ItemCandidate itemCandidate)
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            Item item = new()
            {
                Name = itemCandidate.Name!,
                CollectionId = collection!.Id,
                ImageLink = itemCandidate.ImageLink,
            };

            adc.Items.Add(item);
            adc.SaveChanges();
        }
    }

    private async Task CreateNewItem()
    {
        CreateItem(Model!);
        newItemRequested = !newItemRequested;
        Model ??= new();
        itemsBunch = [];
        var t = CreateData2().ToList<Collection>();
        collection = t.First(x => x.Id == Id);
        itemsBunch.AddRange(collection.Items);
    }

    private async Task CheckAuthorizationLevel()
    {
        AuthenticationState authenticationState = Task.Run(() =>
            _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        ThisUser = Task.Run(() =>
            _UserManager.GetUserAsync(authenticationState.User)).Result;
        if (ThisUser is not null)
        {
            bool userInRoleBlocked = Task.Run(() =>
                _UserManager.IsInRoleAsync(ThisUser, roleBlocked)).Result;
            bool userIsBlocked = ThisUser.LockoutEnd is not null;
            if (userInRoleBlocked || userIsBlocked)
            {
                await _SignInManager.SignOutAsync();
                _navigationManager.NavigateTo(loginPageURL);
            }
        }
    }

    private async Task SubmitNewItem()
    {
        Model!.ImageLink = TempImg;
        Model!.CollectionId = collection!.Id;
        Console.WriteLine(Model?.Name);
        Console.WriteLine(Model?.CollectionId);
        Console.WriteLine(Model?.ImageLink);
        await CreateNewItem();
        newItemRequested = false;
    }

    private void RequestNewItem()
    {
        newItemRequested = !newItemRequested;
    }
}