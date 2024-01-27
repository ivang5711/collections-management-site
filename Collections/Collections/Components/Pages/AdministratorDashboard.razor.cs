using Collections.Data;
using Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Collections.Components.Pages;

public partial class AdministratorDashboard
{
    private const string roleMember = "Member";
    private const string roleAdmin = "Admin";
    private const string roleBlocked = "Blocked";
    private const string roleLockedMessage = "Blocked";
    private const string roleMemberMessage = "Active";
    private const string loginPageURL = "/Account/Login";

    private ElementReference InputToToggle;

    private bool DeleteRequested { get; set; } = false;
    private bool SubmitRequested { get; set; } = false;
    private bool CheckAll { get; set; } = false;
    private List<ApplicationUser> Users { get; set; } = [];
    private bool Ticked { get; set; } = false;
    private List<ViewUser> ViewUsers { get; set; } = [];

    private async Task ToggleMe()
    {
        await JsRuntime.InvokeVoidAsync("toggleCheckbox", InputToToggle);
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

    protected override async Task OnInitializedAsync()
    {
        if (await CheckAuthorizationLevel())
        {
            _navigationManager.NavigateTo("/", true);
            return;
        }

        GetUsers();
        int i = 0;
        foreach (var user in Users)
        {
            var t = user.LockoutEnd;
            ViewUser viewUser = new();
            viewUser.Id = ++i;
            viewUser.Name = user.FullName ?? "no name provided";
            viewUser.Email = user.Email ?? "no Email provided";
            viewUser.LastLoginDate = user.LastLoginDate.ToString();
            viewUser.RegistrationDate = user.RegistrationDate.ToString();
            string viewRole = string.Empty;
            if (await _UserManager.IsInRoleAsync(user, "ToRemove"))
            {
                viewRole = "ToRemove";
            }
            else if (await _UserManager.IsInRoleAsync(user, roleAdmin))
            {
                viewRole = roleAdmin;
            }
            else if (await _UserManager.IsInRoleAsync(user, roleMember))
            {
                viewRole = roleMember;
            }

            viewUser.Role = viewRole;
            if (user.LockoutEnd is null)
            {
                viewUser.Status = roleMemberMessage;
            }
            else
            {
                viewUser.Status = roleLockedMessage;
            }

            ViewUsers.Add(viewUser);
        }
    }

    private void GetUsers()
    {
        Users.Clear();
        using var adc = _contextFactory.CreateDbContext();
        Users.AddRange(adc.Users.AsEnumerable());
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

    private void SetLoadingOnDelete()
    {
        DeleteRequested = false;
        SubmitRequested = true;
    }

    private void RequestDelete()
    {
        DeleteRequested = true;
    }

    private async Task Submit()
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

    private void CloseModal()
    {
        DeleteRequested = false;
    }

    private void SelectAll()
    {
        CheckAll = !CheckAll;
        foreach (var item in ViewUsers)
        {
            if (CheckAll)
            {
                item.IsChecked = true;
            }
            else
            {
                item.IsChecked = false;
            }
        }
    }

    private void RefreshPage()
    {
        _navigationManager.Refresh(false);
    }

    private void BlockUser()
    {
        var vu = ViewUsers.Where(x => x.IsChecked == true).ToList();
        List<ApplicationUser> names = [];
        foreach (var item in vu)
        {
            names.Add(Users.Where(x => x.UserName == item.Email).First());
        }

        foreach (var item in names)
        {
            BlockAUser(item);
        }

        RefreshPage();
    }

    private void BlockAUser(ApplicationUser user)
    {
        SetUserLockOutState(user);

        var us = Task.Run(() => _AuthenticationStateProvider
            .GetAuthenticationStateAsync()).Result;

        var t = us.User;
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
        using (var adc = _contextFactory.CreateDbContext())
        {
            var p = adc.Users.First(x => x.Id == user.Id);
            p.LockoutEnd = DateTime.MaxValue;
            adc.SaveChanges();
        }

        _UserManager.AddToRoleAsync(user, roleBlocked);
    }

    private void UnblockUser()
    {
        var vu = ViewUsers.Where(x => x.IsChecked == true).ToList();
        List<ApplicationUser> names = [];
        foreach (var item in vu)
        {
            names.Add(Users.Where(x => x.UserName == item.Email).First());
        }

        foreach (var item in names)
        {
            UnblockAUser(item);
        }

        RefreshPage();
    }

    private void UnblockAUser(ApplicationUser user)
    {
        user.LockoutEnd = null;

        using (var adc = _contextFactory.CreateDbContext())
        {
            var p = adc.Users.First(x => x.Id == user.Id);
            p.LockoutEnd = null;
            adc.SaveChanges();
        }

        _UserManager.RemoveFromRoleAsync(user, roleBlocked);
    }

    private async Task AddUserToAdmins()
    {
        var vu = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        foreach (var item in vu)
        {
            names.Add(Users.First(x => x.UserName == item.Email));
        }

        foreach (var item in names)
        {
            if (!(await _UserManager.IsInRoleAsync(item, roleAdmin)))
            {
                AddAUserToAdmins(item);
            }
        }

        RefreshPage();
    }

    private void AddAUserToAdmins(ApplicationUser user)
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var r = adc.Roles.First(x => x.Name == roleAdmin);
            var m = adc.UserRoles.Where(x => x.UserId == user.Id).First();
            var d = m;
            d.RoleId = r.Id;
            adc.Add(d);
            adc.SaveChanges();
        }
    }

    private async Task RemoveUserFromAdmins()
    {
        var vu = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names2 = [];
        foreach (var item in vu)
        {
            names2.Add(Users.First(x => x.UserName == item.Email));
        }

        foreach (var item in names2)
        {
            if (await _UserManager.IsInRoleAsync(item, roleAdmin))
            {
                RemoveAUserFromAdmins(item);
            }
        }

        RefreshPage();
    }

    private void RemoveAUserFromAdmins(ApplicationUser user)
    {
        using (var adc = _contextFactory.CreateDbContext())
        {
            var p = adc.Users.First(x => x.Id == user.Id);
            var r = adc.Roles.First(x => x.Name == roleAdmin);
            var m = adc.UserRoles.Where(x => x.UserId == p.Id).First(x => x.RoleId == r.Id);
            adc.Remove(m);
            adc.SaveChanges();
        }

        var us = Task.Run(() => _AuthenticationStateProvider
            .GetAuthenticationStateAsync()).Result;
        var t = us.User;
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
        var vu = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        foreach (var item in vu)
        {
            names.Add(Users.First(x => x.UserName == item.Email));
        }

        foreach (var item in names)
        {
            DeleteAUser(item);
        }
    }

    private void DeleteAUser(ApplicationUser user)
    {
        SetUserLockOutState(user);
        using (var adc = _contextFactory.CreateDbContext())
        {
            var r = adc.Roles.First(x => x.Name == "ToRemove");
            var m = adc.UserRoles.Where(x => x.UserId == user.Id).First();
            var d = m;
            d.RoleId = r.Id;
            adc.Add(d);
            adc.SaveChanges();
        }

        var us = Task.Run(() => _AuthenticationStateProvider
            .GetAuthenticationStateAsync()).Result;
        var t = us.User;
        if (user.UserName == t.Identity!.Name)
        {
            _navigationManager.NavigateTo("/", true);
            _ = Task.Run(() => _SignInManager.SignOutAsync());
            return;
        }
    }
}