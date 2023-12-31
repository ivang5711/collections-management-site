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

    private const string dateTimeViewFormatString =
        "HH':'mm':'ss, d MMM, yyyy";

    private const string claimTypeRegistrationDateTime =
        "RegistrationDateTime";

    ElementReference InputToToggle;

    private bool DeleteRequested { get; set; } = false;
    private bool SubmitRequested { get; set; } = false;
    private bool CheckAll { get; set; } = false;
    private List<ApplicationUser> Users { get; set; } = [];
    private bool Ticked { get; set; } = false;
    private List<ViewUser> ViewUsers { get; set; } = [];

    async Task ToggleMe()
    {
        await JsRuntime.InvokeVoidAsync("toggleIt", InputToToggle);
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

        //var us = Task.Run(() =>
        //    _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        //var tryy = Task.Run(() => _UserManager.GetUserAsync(us.User)).Result;
        //var ew = Task.Run(() =>
        //    _UserManager.IsInRoleAsync(tryy, roleAdmin)).Result;
        //var io = tryy.LockoutEnd is not null;
        //if (!ew)
        //{
        //    _ = Task.Run(() => _SignInManager.SignOutAsync());
        //    _navigationManager.NavigateTo("/Account/Login");
        //    return;
        //}

        //if (io)
        //{
        //    _ = Task.Run(() => _SignInManager.SignOutAsync());
        //    _navigationManager.NavigateTo("/Account/Login");
        //    return;
        //}

        //var authenticationState = Task.Run(() =>
        //    _AuthenticationStateProvider.GetAuthenticationStateAsync()).Result;
        //var currentUser = Task.Run(() =>
        //    _UserManager.GetUserAsync(authenticationState.User)).Result;
        //if (currentUser is not null)
        //{
        //    var userInRoleBlocked = Task.Run(() =>
        //        _UserManager.IsInRoleAsync(currentUser, roleBlocked)).Result;
        //    var userIsBlocked = currentUser.LockoutEnd is not null;
        //    if (userInRoleBlocked || userIsBlocked)
        //    {
        //        await _SignInManager.SignOutAsync();
        //        _navigationManager.NavigateTo("/Account/Login");
        //        return;
        //    }
        //}


        GetUsers();
        int i = 0;
        foreach (var user in Users)
        {
            var t = user.LockoutEnd;
            _UserManager.SetLockoutEndDateAsync(user, t).Wait();
            ViewUser viewUser = new();
            viewUser.Id = ++i;
            viewUser.Name = user.UserName ?? "no name provided";
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
        Users.AddRange(_UserManager.Users.AsEnumerable());
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

    public void SubmitAddToAdmins()
    {
        SubmitRequested = true;
        AddUserToAdmins();
    }

    public void SubmitRemoveFromAdmins()    
    {
        SubmitRequested = true;
        RemoveUserFromAdmins();
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

        ViewUsers.Clear();
        GetUsers();
        int i = 0;
        foreach (var user in Users)
        {
            var t = user.LockoutEnd;
            _UserManager.SetLockoutEndDateAsync(user, t).Wait();
            ViewUser viewUser = new();
            viewUser.Id = ++i;
            viewUser.Name = user.UserName ?? "no name provided";
            viewUser.Email = user.Email ?? "no Email provided";
            viewUser.LastLoginDate = user.LastLoginDate.ToString();
            viewUser.RegistrationDate = user.RegistrationDate.ToString();
            string viewRole = string.Empty;
            if (await _UserManager.IsInRoleAsync(user, "ToRemove"))
            {
                viewRole = "ToRemove";
            }
            else if (Task.Run(() =>
                _UserManager.IsInRoleAsync(user, roleAdmin)).Result)
            {
                viewRole = roleAdmin;
            }
            else if (Task.Run(() =>
                _UserManager.IsInRoleAsync(user, roleMember)).Result)
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
            Thread.Sleep(300);
        }

        if (CheckAll)
        {
            CheckAll = !CheckAll;
            //await ToggleMe();
        }

        SubmitRequested = false;
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
            names.Add(Users.Where(x => x.UserName == item.Name).First());
        }

        foreach (var item in names)
        {
            BlockAUser(item);
        }

        RefreshPage();
    }

    private void BlockAUser(ApplicationUser user)
    {
        user.LockoutEnd = DateTime.MaxValue;
        _ = Task.Run(() => _UserManager
            .SetLockoutEndDateAsync(user, DateTime.MaxValue)).Result;
        _ = Task.Run(() => _UserManager
            .AddToRoleAsync(user, roleBlocked)).Result;

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

    private void UnblockUser()
    {
        var vu = ViewUsers.Where(x => x.IsChecked == true).ToList();
        List<ApplicationUser> names = [];
        foreach (var item in vu)
        {
            names.Add(Users.Where(x => x.UserName == item.Name).First());
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
        _ = Task.Run(() => _UserManager
            .SetLockoutEndDateAsync(user, null)).Result;
        _ = Task.Run(() => _UserManager
            .RemoveFromRoleAsync(user, roleBlocked)).Result;
    }

    private void AddUserToAdmins()
    {
        var vu = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names = [];
        foreach (var item in vu)
        {
            names.Add(Users.First(x => x.UserName == item.Name));
        }

        foreach (var item in names)
        {
            AddAUserToAdmins(item);
        }

        RefreshPage();
    }

    private void AddAUserToAdmins(ApplicationUser user)
    {
        _ = Task.Run(() => _UserManager
            .AddToRoleAsync(user, roleAdmin)).Result;
    }

    private void RemoveUserFromAdmins()
    {
        var vu = ViewUsers.Where(x => x.IsChecked).ToList();
        List<ApplicationUser> names2 = [];
        foreach (var item in vu)
        {
            names2.Add(Users.First(x => x.UserName == item.Name));
        }

        foreach (var item in names2)
        {
            RemoveAUserFromAdmins(item);
        }

        RefreshPage();
    }

    private void RemoveAUserFromAdmins(ApplicationUser user)
    {
        _ = Task.Run(() => _UserManager
            .RemoveFromRoleAsync(user, roleAdmin)).Result;
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
            names.Add(Users.First(x => x.UserName == item.Name));
        }

        foreach (var item in names)
        {
            DeleteAUser(item);
        }

    }

    private void DeleteAUser(ApplicationUser user)
    {
        user.LockoutEnd = DateTime.MaxValue;
        _ = Task.Run(() => _UserManager
            .SetLockoutEndDateAsync(user, DateTime.MaxValue)).Result;

        IEnumerable<string> ro = [roleBlocked, "ToRemove"];

        _ = Task.Run(() => _UserManager
            .AddToRolesAsync(user, ro)).Result;

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