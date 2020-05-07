using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core;
using EZms.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EZms.UI.Areas.EZms.Pages.Admin
{
    [Authorize(Roles = "EZmsAdmin,Administrators")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EZmsContext _context;
        public IEnumerable<IdentityUser> Users { get; set; }

        public IndexModel(UserManager<IdentityUser> userManager, EZmsContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Users = await _context.Users.OrderBy(w => w.Email).ToListAsync();
        }
    }
}