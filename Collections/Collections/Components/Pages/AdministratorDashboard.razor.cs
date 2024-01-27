using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Collections.Components.Pages;

public partial class AdministratorDashboard
{
    private const string roleMember = "Member";
    private const string roleAdmin = "Admin";
    private const string roleBlocked = "Blocked";
    private const string roleLockedMessage = "Blocked";
    private const string roleMemberMessage = "Active";
    private const string loginPageURL = "/Account/Login";

    private bool DeleteRequested { get; set; } = false;
    private bool SubmitRequested { get; set; } = false;
    private bool CheckAll { get; set; } = false;
    private List<ApplicationUser> Users { get; set; } = [];
    private bool Ticked { get; set; } = false;
    private List<ViewUser> ViewUsers { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        if (await CheckAuthorizationLevel())
        {
            _navigationManager.NavigateTo("/", true);
            return;
        }

        await InitializeData();
    }

    private async Task InitializeData()
    {
        GetUsers();
        await InitializeViewUsers();
    }

    private async Task InitializeViewUsers()
    {
        int i = 0;
        foreach (var user in Users)
        {
            await InitializeSingleViewUser(user, ++i);
        }
    }

    private async Task InitializeSingleViewUser(ApplicationUser user, int i)
    {
        ViewUser viewUser = new()
        {
            Id = i,
            Name = user.FullName ?? "no name provided",
            Email = user.Email ?? "no Email provided",
            LastLoginDate = user.LastLoginDate.ToString(),
            RegistrationDate = user.RegistrationDate.ToString(),
            Role = await SetViewUserRole(user),
            Status = (user.LockoutEnd is null) ?
            roleMemberMessage : roleLockedMessage
        };

        ViewUsers.Add(viewUser);
    }

    private async Task<string> SetViewUserRole(ApplicationUser user)
    {
        if (await _UserManager.IsInRoleAsync(user, "ToRemove"))
        {
            return "ToRemove";
        }
        else if (await _UserManager.IsInRoleAsync(user, roleAdmin))
        {
            return roleAdmin;
        }
        else if (await _UserManager.IsInRoleAsync(user, roleMember))
        {
            return roleMember;
        }

        return string.Empty;
    }

    private void GetUsers()
    {
        Users.Clear();
        using var ctx = _contextFactory.CreateDbContext();
        Users.AddRange(ctx.Users.AsEnumerable());
    }

    public void SubmitBlockUser()
    {
        SubmitRequested = true;
        BlockUser();
    }

    public void SubmitUnblockUser()
    {
        SubmitRequested = true;
        UnblockUser();
    }

    public async Task SubmitAddToAdmins()
    {
        SubmitRequested = true;
        await AddUserToAdmins();
    }

    public async Task SubmitRemoveFromAdmins()
    {
        SubmitRequested = true;
        await RemoveUserFromAdmins();
    }

    private void SubmitDeleteUser()
    {
        DeleteRequested = false;
        SubmitRequested = true;
        DeleteUser();
    }

    private void Submit()
    {
        if (DeleteRequested)
        {
            SubmitDeleteUser();
        }

        if (CheckAll)
        {
            CheckAll = !CheckAll;
        }

        SubmitRequested = false;
        _navigationManager.Refresh(true);
    }

    private void SelectAll()
    {
        CheckAll = !CheckAll;
        foreach (var item in ViewUsers)
        {
            item.IsChecked = CheckAll;
        }
    }

    private void BlockUser()
    {
        var viewUser = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        GetCheckedUsers(viewUser, ref names);
        foreach (var item in names)
        {
            BlockAUser(item);
        }
    }

    private void BlockAUser(ApplicationUser user)
    {
        SetUserLockOutState(user);
        var authenticationState = Task.Run(() => _AuthenticationStateProvider
            .GetAuthenticationStateAsync()).Result;
        var t = authenticationState.User;
        if (user.UserName == t.Identity!.Name)
        {
            _navigationManager.Refresh(true);
            _ = Task.Run(() => _SignInManager.SignOutAsync());
            return;
        }
    }

    private void SetUserLockOutState(ApplicationUser user)
    {
        user.LockoutEnd = DateTime.MaxValue;
        using (var ctx = _contextFactory.CreateDbContext())
        {
            var item = ctx.Users.First(x => x.Id == user.Id);
            item.LockoutEnd = DateTime.MaxValue;
            ctx.SaveChanges();
        }

        _UserManager.AddToRoleAsync(user, roleBlocked);
    }

    private void UnblockUser()
    {
        var viewUser = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        GetCheckedUsers(viewUser, ref names);
        foreach (var item in names)
        {
            UnblockAUser(item);
        }
    }

    private void UnblockAUser(ApplicationUser user)
    {
        user.LockoutEnd = null;
        using (var ctx = _contextFactory.CreateDbContext())
        {
            var item = ctx.Users.First(x => x.Id == user.Id);
            item.LockoutEnd = null;
            ctx.SaveChanges();
        }

        _UserManager.RemoveFromRoleAsync(user, roleBlocked);
    }

    private async Task AddUserToAdmins()
    {
        var viewUser = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        GetCheckedUsers(viewUser, ref names);
        foreach (var item in names)
        {
            if (!(await _UserManager.IsInRoleAsync(item, roleAdmin)))
            {
                AddAUserToAdmins(item);
            }
        }
    }

    private void AddAUserToAdmins(ApplicationUser user)
    {
        using var ctx = _contextFactory.CreateDbContext();
        var r = ctx.Roles.First(x => x.Name == roleAdmin);
        var m = ctx.UserRoles.Where(x => x.UserId == user.Id).First();
        var d = m;
        d.RoleId = r.Id;
        ctx.Add(d);
        ctx.SaveChanges();
    }

    private async Task RemoveUserFromAdmins()
    {
        var viewUser = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        GetCheckedUsers(viewUser, ref names);
        foreach (var item in names)
        {
            if (await _UserManager.IsInRoleAsync(item, roleAdmin))
            {
                RemoveAUserFromAdmins(item);
            }
        }
    }

    private void RemoveAUserFromAdmins(ApplicationUser user)
    {
        using (var ctx = _contextFactory.CreateDbContext())
        {
            var p = ctx.Users.First(x => x.Id == user.Id);
            var r = ctx.Roles.First(x => x.Name == roleAdmin);
            var m = ctx.UserRoles.Where(x => x.UserId == p.Id).First(x => x.RoleId == r.Id);
            ctx.Remove(m);
            ctx.SaveChanges();
        }

        CheckIfDeleteCurrentUser(user);
    }

    private void CheckIfDeleteCurrentUser(ApplicationUser user)
    {
        var authenticationState = Task.Run(() => _AuthenticationStateProvider
            .GetAuthenticationStateAsync()).Result;
        var t = authenticationState.User;
        if (user.UserName == t.Identity!.Name)
        {
            _ = Task.Run(() => _SignInManager.SignOutAsync());
            _ = _SignInManager.SignOutAsync();
            _navigationManager.Refresh(true);
            return;
        }
    }

    private void DeleteUser()
    {
        var viewUser = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        GetCheckedUsers(viewUser, ref names);
        foreach (var item in names)
        {
            DeleteAUser(item);
        }
    }

    private void GetCheckedUsers(List<ViewUser> viewUsers, ref List<ApplicationUser> names)
    {
        foreach (var item in viewUsers)
        {
            names.Add(Users.First(x => x.UserName == item.Email));
        }
    }

    private void DeleteAUser(ApplicationUser user)
    {
        SetUserLockOutState(user);
        using (var ctx = _contextFactory.CreateDbContext())
        {
            var r = ctx.Roles.First(x => x.Name == "ToRemove");
            var m = ctx.UserRoles.Where(x => x.UserId == user.Id).First();
            var d = m;
            d.RoleId = r.Id;
            ctx.Add(d);
            ctx.SaveChanges();
        }

        SignOutCurrentUserIfDeleted(user);
    }

    private void SignOutCurrentUserIfDeleted(ApplicationUser user)
    {
        var authenticationState = Task.Run(() => _AuthenticationStateProvider
            .GetAuthenticationStateAsync()).Result;
        var t = authenticationState.User;
        if (user.UserName == t.Identity!.Name)
        {
            _navigationManager.NavigateTo("/", true);
            _ = Task.Run(() => _SignInManager.SignOutAsync());
            return;
        }
    }

    private async Task<bool> CheckAuthorizationLevel()
    {
        AuthenticationState authenticationState = Task.Run(() =>
            _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        ApplicationUser? currentUser = Task.Run(() =>
            _UserManager.GetUserAsync(authenticationState.User)).Result;
        if (currentUser is not null)
        {
            bool userInRoleBlocked = Task.Run(() =>
                _UserManager.IsInRoleAsync(currentUser, roleBlocked)).Result;
            bool userInRoleAdmin = Task.Run(() =>
                _UserManager.IsInRoleAsync(currentUser, roleAdmin)).Result;
            bool userIsBlocked = currentUser.LockoutEnd is not null;
            if (userInRoleBlocked || userIsBlocked || !userInRoleAdmin)
            {
                await _SignInManager.SignOutAsync();
                _navigationManager.NavigateTo(loginPageURL);
                return true;
            }
        }
        else
        {
            _navigationManager.NavigateTo("/", true);
            return true;
        }

        return false;
    }
}