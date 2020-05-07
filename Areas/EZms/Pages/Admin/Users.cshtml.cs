using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core;
using EZms.Core.Cache;
using EZms.Core.Extensions;
using EZms.UI.Areas.EZms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EZms.UI.Areas.EZms.Pages.Admin
{
    [Authorize(Roles = "EZmsAdmin,Administrators")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EZmsContext _context;
        public IEnumerable<IdentityUser> Users { get; private set; }
        public IdentityUser SelectedUser { get; set; }
        public IEnumerable<string> SelectedUserRoles { get; private set; }
        public IEnumerable<string> AllRoles { get; private set; }

        public UsersModel(UserManager<IdentityUser> userManager, EZmsContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync(string id)
        {
            await Init(id);
        }

        public async Task<IActionResult> OnPostAsync(string id, UserPostModel model, IFormCollection collection)
        {
            await Init(id);

            if (!ModelState.IsValid)
                return Page();

            var unselectedRoles = SelectedUserRoles.Where(r => !model.SelectedRoles?.Contains(r) ?? false).ToList();
            var newRoles = model.SelectedRoles?.Where(r => !SelectedUserRoles.Contains(r)).ToList();
            var rolesUpdated = false;

            if (unselectedRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(SelectedUser, unselectedRoles);
                rolesUpdated = true;
            }

            if (!newRoles.IsNullOrEmpty())
            {
                await _userManager.AddToRolesAsync(SelectedUser, newRoles);
                rolesUpdated = true;
            }

            if (rolesUpdated)
            {
                EZmsInMemoryCache.Remove($"EZms:principal:userroles:{id}");
            }

            SelectedUser.Email = model.Email;
            SelectedUser.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(SelectedUser);

            if (unselectedRoles.Any() || newRoles.Any())
                SelectedUserRoles = await GetUserRoles(SelectedUser);

            return Page();
        }

        public async Task<IActionResult> OnGetLockAccountAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return new JsonResult(new { redirectUrl = Url.Page("Users", new { id }) });
        }

        public async Task<IActionResult> OnGetUnlockAccountAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);

            return new JsonResult(new { redirectUrl = Url.Page("Users", new { id }) });
        }

        public async Task<IActionResult> OnGetDeleteAccountAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
            return new JsonResult(new { redirectUrl = Url.Page("Users", new { id = "" }) });
        }

        private async Task Init(string id)
        {
            if (string.IsNullOrEmpty(id))
                Users = await _context.Users.OrderBy(w => w.UserName).ToListAsync();
            else
            {
                SelectedUser = await _userManager.FindByIdAsync(id);
                if (SelectedUser != null)
                    SelectedUserRoles = await GetUserRoles(SelectedUser);
                AllRoles = await _context.Roles.Select(w => w.Name).ToListAsync();
            }
        }

        private async Task<IEnumerable<string>> GetUserRoles(IdentityUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}