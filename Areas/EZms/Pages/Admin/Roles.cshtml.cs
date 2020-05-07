using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EZms.UI.Areas.EZms.Pages.Admin
{
    [Authorize(Roles = "EZmsAdmin,Administrators")]
    public class RolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EZmsContext _context;
        public IEnumerable<IdentityUser> Users { get; set; }
        public IEnumerable<string> AllRoles { get; set; }
        public string SelectedRole { get; set; }

        public RolesModel(UserManager<IdentityUser> userManager, EZmsContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync(string role)
        {
            if (string.IsNullOrEmpty(role))
                await SetAllRoles();
            else
            {
                SelectedRole = role;
                Users = await _userManager.GetUsersInRoleAsync(role);
            }
        }

        public async Task OnPostAsync(IFormCollection formCollection)
        {
            var roleName = formCollection["NewRoleName"].ToString();
            var role = new IdentityRole(roleName) { NormalizedName = roleName.ToUpperInvariant() };

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            await SetAllRoles();
        }

        public async Task OnDeleteAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role != null)
            {
                _context.Remove(role);
                await _context.SaveChangesAsync();
            }

            await SetAllRoles();
        }

        [NonAction]
        private async Task SetAllRoles()
        {
            AllRoles = await _context.Roles.Select(w => w.Name).ToListAsync();
        }
    }
}